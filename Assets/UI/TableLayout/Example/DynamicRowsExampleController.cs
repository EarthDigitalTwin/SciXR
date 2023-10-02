/*
 * This example shows how you can add rows programmatically.
 */ 
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UI.Tables;

namespace UI.Tables.Examples
{
    public class DynamicRowsExampleController : MonoBehaviour
    {
        // this is the TableLayout which we will be adding rows to
        // (in this example, this TableLayout is nested within a ScrollView)
        public TableLayout tableLayout;
        
        // this is a template which we will be using as a base for the new rows
        public TableRow rowTemplate;

        // this is the content transform of the ScrollView (aka ScrollRect)
        public RectTransform scrollviewContent;

        // this controls how many test rows will be added to the TableLayout
        public int numberOfRowsToAdd = 50;
        private int numberOfRowsAdded = 0;

        // Font for the dynamic rows example
        public Font font;

        void OnEnable()
        {
            // This doesn't have to be done with a coroutine, this is just so that the example runs slowly enough so that you can see each row being added
            // (normally we'd probably add them all at once)
            StartCoroutine(AddRowsUsingTemplate());

            // To see how to add rows without using a template, uncomment the following line (and comment out the above one for preference)
            //StartCoroutine(AddRowsWithoutTemplate());
        }

        IEnumerator AddRowsUsingTemplate()
        {
            while (numberOfRowsAdded <= numberOfRowsToAdd)
            {
                // Create a new row based on our template
                var newRow = Instantiate<TableRow>(rowTemplate);

                // the template is inactive, so we need to make our new row active
                newRow.gameObject.SetActive(true);

                // add the new row to the table
                tableLayout.AddRow(newRow);                

                // In order for the scrollview to work correctly, its content transform must have the same height as our table
                scrollviewContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (tableLayout.transform as RectTransform).rect.height);
                
                // set the text value in the first cell
                var textObject = newRow.Cells[0].GetComponentInChildren<Text>();
                textObject.text = "Row " + numberOfRowsAdded;

                // set the value for the toggle element in the second cell (even rows on, odd rows off)
                var toggleObject = newRow.Cells[1].GetComponentInChildren<Toggle>();
                toggleObject.isOn = numberOfRowsAdded % 2 == 0;

                // increment the number of rows added (so that the example doesn't continue forever)
                numberOfRowsAdded++;

                // wait briefly before adding the next row
                yield return new WaitForSeconds(0.25f);
            }
        }

        IEnumerator AddRowsWithoutTemplate()
        {
            while (numberOfRowsAdded <= numberOfRowsToAdd)
            {                
                // Add a row with 0 cells (we'll be adding the cells manually)
                var newRow = tableLayout.AddRow(0);

                // the default behaviour of a row is to be autosized based on the available space; 
                // in this case we want to explicitly set the row height and have the table's height adjusted to match
                newRow.preferredHeight = 72;

                // In order for the scrollview to work correctly, its content transform must have the same height as our table
                scrollviewContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (tableLayout.transform as RectTransform).rect.height);

                // Add some cell content
                // (Generally, you're better off using a prefab or other object as a template, but for this example we'll create an entirely new text object for our cell)
                var textGameObject = new GameObject("Text", typeof(RectTransform));                
                                
                newRow.AddCell(textGameObject.transform as RectTransform);                
                
                var textObject = textGameObject.AddComponent<Text>();
                textObject.font = font; // failing to set a font for a new text object triggers a null exception
                textObject.text = "Row " + numberOfRowsAdded;
                textObject.resizeTextForBestFit = true;
                textObject.alignment = TextAnchor.MiddleCenter;


                // and two empty cells
                newRow.AddCell();
                newRow.AddCell();                                

                // increment the number of rows added (so that the example doesn't continue forever)
                numberOfRowsAdded++;

                // wait briefly before adding the next row
                yield return new WaitForSeconds(0.25f);
            }
        }


    }
}
