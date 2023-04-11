using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageObjectCollection : MonoBehaviour
{
    #region Public Variables
    public float xPosition = 0;
    public float startingYPosition = 0;
    public float deltaYPosition = 0;
    public float zPosition = 0;

    public int objectsPerPage = 1;
    public bool wrapAround = false;
    #endregion

    #region Private Variables
    [SerializeField]
    private List<GameObject> objects = new List<GameObject>();
    private int currentPage;
    private int numPages;
    #endregion 

    public void SetUp(List<GameObject> objectList)
    {
        currentPage = 0;

        objects.Clear();
        objects = objectList;
        foreach (GameObject obj in objects)
        {
            obj.SetActive(false);
        }

        numPages = (int)Mathf.Ceil(objects.Count / objectsPerPage);

        SetUpCurrentPage(true);
    }

    private void SetUpCurrentPage(bool enable)
    {
        if (enable)
        {
            float currY = startingYPosition;

            for (int i = currentPage * objectsPerPage; i < currentPage * objectsPerPage + objectsPerPage; i++)
            {
                if (i > objects.Count - 1)
                    break;

                GameObject curr = objects[i];

                curr.SetActive(true);
                curr.transform.localPosition = new Vector3(xPosition, currY, zPosition);

                currY -= deltaYPosition;
            }
        }
        else
        {
            for (int i = currentPage * objectsPerPage; i < currentPage * objectsPerPage + objectsPerPage; i++)
            {
                if (i > objects.Count - 1)
                    break;

                objects[i].SetActive(false);
            }
        }
    }

    public void Left()
    {
        if (currentPage == 0)
        {
            if (wrapAround)
            {
                SetUpCurrentPage(false);
                currentPage = numPages;
                SetUpCurrentPage(true);
            }
        }
        else
        {
            SetUpCurrentPage(false);
            currentPage -= 1;
            SetUpCurrentPage(true);
        }
    }

    public void Right()
    {
        if (currentPage == numPages)
        {
            if (wrapAround)
            {
                SetUpCurrentPage(false);
                currentPage = 0;
                SetUpCurrentPage(true);
            }
        }
        else
        {
            SetUpCurrentPage(false);
            currentPage += 1;
            SetUpCurrentPage(true);
        }
    }
}
