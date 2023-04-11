﻿/*
Copyright 2006 Jerry Huxtable

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;

namespace Proj4Net.Projection
{

/*
 * This file was semi-automatically converted from the public-domain USGS PROJ source.
 */

public class McBrydeThomasFlatPolarSine1Projection : SineTangentSeriesProjection 
{

	public McBrydeThomasFlatPolarSine1Projection() 
        :base(1.48875, 1.36509, false)
    {
	}
	
	public override String ToString() {
		return "McBryde-Thomas Flat-Polar Sine (No. 1)";
	}

}}