using UnityEngine;
using UnityEditor;

public class CreateAssetbundles
{
    [MenuItem("AssetsBundle/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {

        Object[] SelectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);


        foreach (Object obj in SelectedAsset)
        {

            string targetPath = Application.dataPath + "/AssetBundles/" + obj.name + ".assetbundle";
            if (BuildPipeline.BuildAssetBundle(obj, null, targetPath, BuildAssetBundleOptions.CollectDependencies, BuildTarget.WSAPlayer))
            {
                Debug.Log(obj.name + "Resource packing success");
            }
            else
            {
                Debug.Log(obj.name + "Resource packing fail");
            }
        }
        AssetDatabase.Refresh();
    }
}


