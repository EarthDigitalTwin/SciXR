#!/usr/bin/python3

from datetime import datetime
from pathlib import Path
import xarray as xr
import time
import requests
import numpy as np
import yaml
import argparse

dt_format = "%Y-%m-%dT%H:%M:%SZ"
header = '''ply
format ascii 1.0
comment identifier: {identifier}
comment description: {description}
comment instrument: {instrument}
comment time: {data_time}
comment variable: {variable}
comment max_val: {max_val}
comment min_val: {min_val}
comment x_label: Longitude
comment y_label: Latitude
comment z_label: {variable}
comment val_label: {variable} {units}
element vertex {count}
property float x
property float y
property float z
property float val
element face 0
property list uchar int vertex_index
end_header'''


# Analysis functions
def temporal_mean_prep(var_json: dict) -> xr.DataArray:
    '''
    Formats timeavgmap response into xarray dataarray object
    '''
    lat = []
    lon = []
    for row in var_json['data']:
        for data in row:
            if data['lat'] not in lat:
                lat.append(data['lat'])
            if data['lon'] not in lon:
                lon.append(data['lon'])

    lat.sort()
    lon.sort()

    da = xr.DataArray(
        data=np.zeros((len(lat), len(lon))),
        dims=['lat', 'lon'],
        coords=dict(
            lat=(['lat'], lat),
            lon=(['lon'], lon)
        )
    )

    for row in var_json['data']:
        for data in row:
            da.loc[data['lat'], data['lon']] = data['mean']
    da = da.where(da != -9999, 0)
    return da


def temporal_mean(base_url: str, dataset: str, bb: dict, start_time: datetime, end_time: datetime) -> xr.DataArray:
    '''
    Makes request to timeAvgMapSpark endpoint
    '''
    url = f'{base_url}/timeAvgMapSpark?ds={dataset}&' \
          f'b={bb["min_lon"]},{bb["min_lat"]},{bb["max_lon"]},{bb["max_lat"]}&' \
          f'startTime={start_time.strftime(dt_format)}&endTime={end_time.strftime(dt_format)}'

    print('url\n', url)
    print()

    print('Waiting for response from SDAP...\n', end='')
    start = time.perf_counter()
    print(url)
    resp = requests.get(url, verify=False).json()
    print('took {} seconds'.format(time.perf_counter() - start))
    return temporal_mean_prep(resp)


def process_xr(config):
    # Main processing functions
    print("CONFIG HERE:", config)
    # Query SDAP
    data_matrix = temporal_mean(config['sdap_url'],
                                config['dataset'],
                                config['bbox'],
                                datetime.strptime(config['start_time'], '%Y-%m-%d'),
                                datetime.strptime(config['end_time'], '%Y-%m-%d'))
    print('\nEvaluating ' + config['dataset'])
    rows = []
    min_val = 0
    max_val = 0
    previous_val = None
    previous_lat = None
    previous_lon = None
    interp = int(config.get('interpolation', 0))

    for lat_i, lat in enumerate(data_matrix):
        previous_lat = float(data_matrix.coords['lat'].values[lat_i])
        for lon_i, val in enumerate(lat):
            data_val = float(val.values)
            if data_val < min_val:
                min_val = data_val
            if data_val > max_val:
                max_val = data_val
            if previous_val is not None:
                # interpolate from previous
                i_val = (data_val - previous_val)/interp
                i_lat = (float(data_matrix.coords['lat'].values[lat_i]) - previous_lat)/interp
                i_lon = (float(data_matrix.coords['lon'].values[lon_i]) - previous_lon)/interp
                for i in range(1, interp):
                    new_val = previous_val + (i_val * i)
                    new_lat = previous_lat + (i_lat * i)
                    new_lon = previous_lon + (i_lon * i)
                    row = '\n' + str(new_lon) + ' ' + str(new_lat) + ' ' + str(new_val) + ' ' + str(new_val)
                    rows.append(row)

            row = '\n' + str(data_matrix.coords['lon'].values[lon_i]) + ' ' + \
                  str(data_matrix.coords['lat'].values[lat_i]) + ' ' + str(val.values) + ' ' + str(val.values)
            rows.append(row)
            previous_val = data_val
            previous_lon = float(data_matrix.coords['lon'].values[lon_i])

    output_file = '' + config['identifier'] + '.ply'
    count = str(len(rows))

    with open(output_file, 'w') as output:
        new_header = header.replace('{identifier}', config['identifier']).replace('{description}', config['description']).replace('{instrument}', config['instrument']).replace('{variable}', config['variables'][0]).replace('{data_time}', config['label_time']).replace('{units}', config['units']).replace('{max_val}', str(max_val)).replace('{min_val}', str(min_val)).replace('{count}', count)
        output.write(new_header)
        for r in rows:
            output.write(r)
        print('\n*** Successfully created ' + output_file)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(
             description='Processing tool for converting SDAP results to 3D point clouds for SciXR.')
    parser.add_argument(
        '--config',
        dest='config',
        help='Config to use',
        action='store')

    args = parser.parse_args()

    if not args.config:
        print('--config is required')
        exit(1)

    print(f'\nUsing {args.config}\n')
    with Path(args.config).open() as f:
        config = yaml.safe_load(f.read())
        print(config)
        process_xr(config)

    exit()
