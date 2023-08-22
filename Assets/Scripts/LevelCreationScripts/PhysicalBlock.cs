using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhysicalBlock
{
    public float length;
    public float height;
    public float colHeight;
    public float colOffset;
    public float direction;
    public Vector2 rightEnd;
    public Vector2 leftEnd;
    Vector2 midPoint;
    public Sprite texture;
    public GameObject blockObj;
    public int blockType;
    public int indivID;
    public int blockID;

    public PhysicalBlock(Vector2 left, Vector2 right, Sprite texture = null, int type = 0, int id = 0, int indivID = 0, float defheight = 4, float colHeight = 2, float colOffset = .5f)
    {
        setPoints(left, right);
        this.texture = texture;
        height = defheight;
        this.colHeight = colHeight;
        this.colOffset = colOffset;
        blockType = type;
        blockID = id;
        this.indivID = indivID;
    }

    public void setPoints(Vector2 left, Vector2 right)
    {
        leftEnd = left;
        rightEnd = right;

        updateAllTransVars();
    }

    public void setHeight(float bHeight)
    {
        height = bHeight;
    }

    public float getHeight()
    {
        return height;
    }

    public void setOffset(float offsetY)
    {
        colOffset = offsetY;
    }

    public float getOffset()
    {
        return colOffset;
    }

    public float getDir()
    {
        //Return simple atan2 direction calculation. SOH CAH TOA Tan = (Opposite/Adgacent)
        direction = Mathf.Atan2(rightEnd.y - leftEnd.y, rightEnd.x - leftEnd.x)/Mathf.PI*180;
        return direction;
    }

    public float getDist()
    {
        //Return simple distance function
        length = Mathf.Abs(Mathf.Sqrt(Mathf.Pow(leftEnd.x - rightEnd.x, 2) + Mathf.Pow(leftEnd.y - rightEnd.y, 2)));
        return length;
    }

    public Vector2 getMidpoint()
    {
        midPoint = (rightEnd + leftEnd) / 2;

        return midPoint;
    }

    public void updateSize()
    {
        blockObj.GetComponent<SpriteRenderer>().size = new Vector2(length, height);
        blockObj.GetComponent<BoxCollider2D>().size = new Vector2(length, colHeight);
        blockObj.GetComponent<BoxCollider2D>().offset = new Vector2(0, colOffset);
    }

    public void updateAllTransVars()
    {
        direction = getDir();
        length = getDist();

        if (blockObj != null)
        {
            blockObj.transform.position = midPoint;
            blockObj.transform.rotation = Quaternion.Euler(blockObj.transform.rotation.x, blockObj.transform.rotation.y, direction);
            updateSize();
        }
    }
}
