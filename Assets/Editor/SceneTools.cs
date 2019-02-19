using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;
using Newtonsoft.Json;
using Deubg = UnityEngine.Debug;

public class SceneTools : EditorWindow
{
    [Serializable]
    public class SceneConfig
    {
        public string sceneId;
        public int width;
        public int height;
        public string textureData;
        public string rotateData;
    }
    [Serializable]
    public class SceneDataCollection
    {
        public Dictionary<string, SceneConfig> sceneDic;
    }

    static SceneDataCollection sceneDataCollection;
    public static string SceneDataPath
    {
        get { return Application.dataPath +  "/../sceneData.txt";  }
    }

    static GameObject parent = null;

    [MenuItem("Tools/OpenSceneToolsWindow")]
    public static void OpenSceneToolsWindow()
    {
        parent = GameObject.Find("Parent");
        if (parent == null)
        {
            parent = new GameObject();
            parent.name = "Parent";
            parent.transform.localPosition = Vector3.zero;
            parent.transform.localEulerAngles = Vector3.zero;
            parent.transform.localScale = Vector3.one;
        }

        EditorWindow.GetWindow(typeof(SceneTools));

        var content = File.ReadAllText(SceneDataPath);
        sceneDataCollection =  JsonConvert.DeserializeObject<SceneDataCollection>(content);
        if (sceneDataCollection == null)
        {
            sceneDataCollection = new SceneDataCollection();
            sceneDataCollection.sceneDic = new Dictionary<string, SceneConfig>();
        }
    }
    
    string sceneId = "1";
    string strSceneSize = "10";
    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("场景ID");
        sceneId = GUILayout.TextField(sceneId);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("场景大小");
        strSceneSize = GUILayout.TextField(strSceneSize);
        GUILayout.EndHorizontal();


        if (GUILayout.Button("CreateNewScene"))
        {
            int size = 10;
            if (int.TryParse(strSceneSize, out size))
            {
                CreateCubes(size);
            }
        }
        if (GUILayout.Button("LoadSceneInfo"))
        {
            //var mat = Resources.Load("0_0") as Material;
            //Debug.Log(mat);

            //Shader shader = Shader.Find("Legacy Shaders/Diffuse");
            //var mat2 = new Material(shader);
            //AssetDatabase.CreateAsset(mat2, "Assets/Resources/0_1.mat");

            LoadSceneInfo(sceneId);
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Save"))
        {
            SceneConfig sceneConfig = null;

            if (sceneDataCollection.sceneDic == null)
                sceneDataCollection.sceneDic = new Dictionary<string, SceneConfig>();

            if (!sceneDataCollection.sceneDic.TryGetValue(sceneId, out sceneConfig))
            {
                sceneConfig = new SceneConfig();
                sceneDataCollection.sceneDic.Add(sceneId, sceneConfig);
            }
            sceneConfig.sceneId = sceneId;

            int size;
            int.TryParse(strSceneSize, out size);

            sceneConfig.width = size;
            sceneConfig.height = size;

            StringBuilder sb = new StringBuilder();
            StringBuilder rotateSB = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    var go = parent.transform.Find(i+"_"+j);
                    sb.Append(go.GetComponent<Renderer>().sharedMaterial.mainTexture.name);
                    sb.Append(',');
                    rotateSB.Append(go.transform.localEulerAngles.z);
                    rotateSB.Append(',');
                }
            }
            sceneConfig.textureData = sb.ToString();
            sceneConfig.rotateData = rotateSB.ToString();

            string ret = JsonConvert.SerializeObject(sceneDataCollection);
            Debug.Log(ret);
            File.WriteAllText(SceneDataPath, ret);
        }

        GUILayout.Space(100);

        if (GUILayout.Button("Rotate"))
        {
            if (Selection.objects.Length <= 0)
                return;

            foreach (var obj in Selection.objects)
            {
                Vector3 angle = (obj as GameObject).transform.localEulerAngles;
                angle.z += 90;
                (obj as GameObject).transform.localEulerAngles = angle;
            }
        }

    }

    void LoadSceneInfo(string sceneId)
    {
        

        SceneConfig sceneConfig = null;
        if (sceneDataCollection.sceneDic.TryGetValue(sceneId, out sceneConfig))
        {
            int size = sceneConfig.height;
            CreateCubes(size);

            strSceneSize = size.ToString();
            string[] data = sceneConfig.textureData.Split(',');
            string[] rotate = sceneConfig.rotateData.Split(',');

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    var go = parent.transform.Find(i + "_" + j);
                    try
                    {
                        go.transform.localEulerAngles = new Vector3(0, 0, float.Parse(rotate[i * size + j]));
                    }
                    catch(Exception)
                    {

                    }
                    var tex = Resources.Load<Texture>("Textures/" + data[i * size + j]);
                    go.GetComponent<Renderer>().sharedMaterial.mainTexture = tex;
                }
            }
            
        }

    }


    //[MenuItem("Tools/CreateCubes")]
    // Start is called before the first frame update
    public static void CreateCubes(int size)
    {
        //for (int i = 0; i < parent.transform.childCount; i++)
        //{
        //    Editor.Destroy(parent.transform.GetChild(i).gameObject);
        //}
        //return;
        int childCount = parent.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Editor.DestroyImmediate(parent.transform.GetChild(0).gameObject);
        }
        for (int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                string objname = string.Format("{0}_{1}", i, j);
                go.name = objname;
                go.transform.parent = parent.transform;
                go.transform.position = new Vector3(j, i, 0);
                go.transform.localEulerAngles = Vector3.zero;
                go.transform.localScale = Vector3.one;

                var mat = Resources.Load("Materials/" + objname) as Material;
                if (mat == null)
                {
                    Shader shader = Shader.Find("Legacy Shaders/Diffuse");
                    mat = new Material(shader);
                    AssetDatabase.CreateAsset(mat, "Assets/Resources/Materials/" + objname + ".mat");
                }
                var texture = Resources.Load("Textures/0");
                mat.mainTexture = texture as Texture;
                go.GetComponent<MeshRenderer>().material = mat;

            }
        }
    }

}
