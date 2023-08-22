using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomLevelDesign))]
public class CustomLevelEditor : Editor
{
    int indexToDelete = 0;

    public override void OnInspectorGUI()
    {
        CustomLevelDesign level = (CustomLevelDesign)target;

        base.OnInspectorGUI();

        //Generate a new physical block.
        if (GUILayout.Button("Generate New Def Block"))
        {
            level.generateNewPhysBlock();
        }

        indexToDelete = EditorGUILayout.IntField("Index to delete", indexToDelete);
        if (GUILayout.Button("Delete Block at index " + indexToDelete))
        {
            level.deletePhysBlock(indexToDelete);
        }

        if (GUILayout.Button("Update all blocks"))
        {
            foreach (PhysicalBlock b in level.allPysicalBlocks)
            {
                b.updateAllTransVars();
            }
        }

        if (GUILayout.Button("Generate Clusters"))
        {
            level.generateClusters();
        }
    }

    
    private void OnSceneGUI()
    {
        CustomLevelDesign level = (CustomLevelDesign)target;

        for (int i = 0; i < level.allPysicalBlocks.Count; i++)
        {
            PhysicalBlock block = level.allPysicalBlocks[i];

            if (block.blockObj == null) continue;

            EditorGUI.BeginChangeCheck();
            Vector2 leftHandle = Handles.PositionHandle(block.leftEnd, Quaternion.identity);
            Vector2 rightHandle = Handles.PositionHandle(block.rightEnd, Quaternion.identity);
            Handles.color = Color.red;
            if (EditorGUI.EndChangeCheck())
            {
                block.setPoints(leftHandle, rightHandle);
            }

            float bHeight = Handles.ScaleHandle(new Vector3(1, block.getHeight(), 1), block.getMidpoint(), block.blockObj.transform.rotation, 1).y;
            if (true)//block.height != bHeight)
            {
                block.setHeight(bHeight);
                block.updateSize();
            }


            Handles.color = Color.green;
            Handles.Label(block.getMidpoint(), "Line " + (i + 1));
        }
    }
}
