using Assets.Scripts;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ItemArray
{
    private Item[,] matrix;
    private int rows;
    private int columns;

    public int Rows => rows;
    public int Columns => columns;

    // Конструктор, который создает матрицу нужного размера
    public ItemArray(int rows, int columns)
    {
        this.rows = rows;
        this.columns = columns;
        this.matrix = new Item[rows, columns];
    }

    public Item this[int row, int column]
    {
        get { return matrix[row, column]; }
        set { matrix[row, column] = value; }
    }

    public void GetRandomRowColumn(out int row, out int column)
    {
        do
        {
            row = random.Next(0, rows);
            column = random.Next(0, columns);
        } while (matrix[row, column] != null);
    }

    public List<ItemMovementDetails> MoveHorizontal(HorizontalMovement horizontalMovement)
    {
        ResetWasJustDuplicatedValues();

        var movementDetails = new List<ItemMovementDetails>();

        int relativeColumn = horizontalMovement == HorizontalMovement.Left ? -1 : 1;
        // Используем локальные columns вместо Globals
        var columnNumbers = Enumerable.Range(0, columns); 

        if (horizontalMovement == HorizontalMovement.Right)
        {
            columnNumbers = columnNumbers.Reverse();
        }

        // Используем локальные rows
        for (int row = rows - 1; row >= 0; row--) 
        {   
            foreach (int column in columnNumbers)
            {
                if (matrix[row, column] == null) continue;

                ItemMovementDetails imd = AreTheseTwoItemsSame(row, column, row, column + relativeColumn);
                if (imd != null)
                {
                    movementDetails.Add(imd);
                    continue;
                }

                int columnFirstNullItem = -1;
                // Используем локальные columns
                int numberOfItemsToTake = horizontalMovement == HorizontalMovement.Left
                ? column : columns - column;

                bool emptyItemFound = false;

                foreach (var tempColumnFirstNullItem in columnNumbers.Take(numberOfItemsToTake))
                {
                    columnFirstNullItem = tempColumnFirstNullItem;
                    if (matrix[row, columnFirstNullItem] == null)
                    {
                        emptyItemFound = true;
                        break;
                    }
                }

                if (!emptyItemFound)
                {
                    continue;
                }

                ItemMovementDetails newImd =
                MoveItemToNullPositionAndCheckIfSameWithNextOne
                (row, row, row, column, columnFirstNullItem, columnFirstNullItem + relativeColumn);

                movementDetails.Add(newImd);
            }
        }
        return movementDetails;
    }

    public List<ItemMovementDetails> MoveVertical(VerticalMovement verticalMovement)
    {
        ResetWasJustDuplicatedValues();

        var movementDetails = new List<ItemMovementDetails>();

        int relativeRow = verticalMovement == VerticalMovement.Bottom ? -1 : 1;
        // Используем локальные rows
        var rowNumbers = Enumerable.Range(0, rows); 

        if (verticalMovement == VerticalMovement.Top)
        {
            rowNumbers = rowNumbers.Reverse();
        }

        // Используем локальные columns
        for (int column = 0; column < columns; column++) 
        {
            foreach (int row in rowNumbers)
            {
                if (matrix[row, column] == null) continue;

                ItemMovementDetails imd = AreTheseTwoItemsSame(row, column, row + relativeRow, column);
                if (imd != null)
                {
                    movementDetails.Add(imd);
                    continue;
                }

                int rowFirstNullItem = -1;
                // Используем локальные rows
                int numberOfItemsToTake = verticalMovement == VerticalMovement.Bottom
                ? row : rows - row;

                bool emptyItemFound = false;

                foreach (var tempRowFirstNullItem in rowNumbers.Take(numberOfItemsToTake))
                {
                    rowFirstNullItem = tempRowFirstNullItem;
                    if (matrix[rowFirstNullItem, column] == null)
                    {
                        emptyItemFound = true;
                        break;
                    }
                }

                if (!emptyItemFound)
                {
                    continue;
                }

                ItemMovementDetails newImd =
                MoveItemToNullPositionAndCheckIfSameWithNextOne(row, rowFirstNullItem, rowFirstNullItem + relativeRow, column, column, column);

                movementDetails.Add(newImd);
            }
        }
        return movementDetails;
    }

    private ItemMovementDetails MoveItemToNullPositionAndCheckIfSameWithNextOne
    (int oldRow, int newRow, int itemToCheckRow, int oldColumn, int newColumn, int itemToCheckColumn)
        {
            //we found a null item, so we attempt the switch ;)
            //bring the first not null item to the position of the first null one
            matrix[newRow, newColumn] = matrix[oldRow, oldColumn];
            matrix[oldRow, oldColumn] = null;

            //check if we have the same value as the left one
            ItemMovementDetails imd2 = AreTheseTwoItemsSame(newRow, newColumn, itemToCheckRow,
                itemToCheckColumn);
            if (imd2 != null)//we have, so add the item returned by the method
            {
                return imd2;
            }
            else//they are not the same, so we'll just animate the current item to its new position
            {
                return
                    new ItemMovementDetails(newRow, newColumn, matrix[newRow, newColumn].GO, null);

            }
        }

    // В методе проверки границ также используем локальные размеры поля
    private ItemMovementDetails AreTheseTwoItemsSame(
        int originalRow, int originalColumn, int toCheckRow, int toCheckColumn)
    {
        if (toCheckRow < 0 || toCheckColumn < 0 || toCheckRow >= rows || toCheckColumn >= columns)
            return null;

        if (matrix[originalRow, originalColumn] != null && matrix[toCheckRow, toCheckColumn] != null
                && matrix[originalRow, originalColumn].Value == matrix[toCheckRow, toCheckColumn].Value
                && !matrix[toCheckRow, toCheckColumn].WasJustDuplicated)
        {
            //double the value, since the item is duplicated
            matrix[toCheckRow, toCheckColumn].Value *= 2;
            matrix[toCheckRow, toCheckColumn].WasJustDuplicated = true;
            //make a copy of the gameobject to be moved and then disappear
            var GOToAnimateScaleCopy = matrix[originalRow, originalColumn].GO;
            //remove this item from the array
            matrix[originalRow, originalColumn] = null;
            return new ItemMovementDetails(toCheckRow, toCheckColumn, matrix[toCheckRow, toCheckColumn].GO, GOToAnimateScaleCopy);

        }
        else
        {
            return null;
        }
    }
    


    private void ResetWasJustDuplicatedValues()
    {
        for (int row = 0; row < Globals.Rows; row++)
            for (int column = 0; column < Globals.Columns; column++)
            {
                if (matrix[row, column] != null && matrix[row, column].WasJustDuplicated)
                    matrix[row, column].WasJustDuplicated = false;
            }
    }

    private System.Random random = new System.Random();
}

public enum HorizontalMovement { Left, Right };
public enum VerticalMovement { Top, Bottom };