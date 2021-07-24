using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ModAPI.Attributes;
using TheForest.Utils;

namespace BuilderCore
{
    public static class Core
    {

        //lists for creating new buildings
        public static Dictionary<int, Building> buildingData;
        public static Dictionary<int, GameObject> prefabs;
        private static uint LastIndex = 0;
        //list for saving all the placed buildings
        public static Dictionary<uint, SerializableBuilding> placedBuildingGameObjects = new Dictionary<uint, SerializableBuilding>();

        private static string GetPath()
        {
            string path = "Mods/Hazard's Mods/BuilderCore/";

            if (GameSetup.IsSinglePlayer)
            {
                path += "Singleplayer saves/";
            }
            else
            {
                path += "Multiplayer saves/";
            }
            path += GameSetup.Slot.ToString();
            return path;

        }


        public static void SavePlacedBuildings()
        {
            try
            {


                MemoryStream stream = new MemoryStream();
                BinaryWriter buf = new BinaryWriter(stream);

                buf.Write(placedBuildingGameObjects.Keys.Count);
                foreach (KeyValuePair<uint,SerializableBuilding> pair in placedBuildingGameObjects)
                {
                    buf.Write(pair.Key);
                    buf.Write(pair.Value.iD);
                    buf.Write(pair.Value.PlacedGameObject.transform.position.x);
                    buf.Write(pair.Value.PlacedGameObject.transform.position.y);
                    buf.Write(pair.Value.PlacedGameObject.transform.position.z);

                    buf.Write(pair.Value.PlacedGameObject.transform.localScale.x);
                    buf.Write(pair.Value.PlacedGameObject.transform.localScale.y);
                    buf.Write(pair.Value.PlacedGameObject.transform.localScale.z);

                    buf.Write(pair.Value.PlacedGameObject.transform.rotation.x);
                    buf.Write(pair.Value.PlacedGameObject.transform.rotation.y);
                    buf.Write(pair.Value.PlacedGameObject.transform.rotation.z);
                    buf.Write(pair.Value.PlacedGameObject.transform.rotation.w);
                }
                buf.Close();
                string path = GetPath();
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                ModAPI.Console.Write("saved " + placedBuildingGameObjects.Keys.Count);
                File.WriteAllBytes(path + "/Building.save", stream.ToArray());
            }
            catch (System.Exception EX)
            {
                ModAPI.Log.Write(EX.ToString());
            }

        }
        public static void LoadPlacedBuildings()
        {
            try
            {

                if (!Directory.Exists(GetPath()))
                {
                    return;
                }

                byte[] bytes = File.ReadAllBytes(GetPath() + "/Building.save");
                var buf = new BinaryReader(new MemoryStream(bytes));

                int buildingCount = buf.ReadInt32();
                for (int i = 0; i < buildingCount; i++)
                {
                    uint index = buf.ReadUInt32();
                    int id = buf.ReadInt32();
                    float posX = buf.ReadSingle();
                    float posY = buf.ReadSingle();
                    float posZ = buf.ReadSingle();

                    float sX = buf.ReadSingle();
                    float sY = buf.ReadSingle();
                    float sZ = buf.ReadSingle();

                    float rX = buf.ReadSingle();
                    float rY = buf.ReadSingle();
                    float rZ = buf.ReadSingle();
                    float rW = buf.ReadSingle();
                    prefabs[id].SetActive(true);

                    GameObject go = GameObject.Instantiate(prefabs[id].gameObject);
                    prefabs[id].SetActive(false);
                    go.transform.position = new Vector3(posX, posY, posZ);
                    go.transform.rotation = new Quaternion(rX, rY, rZ, rW);
                    go.transform.localScale = new Vector3(sX, sY, sZ);
                    SerializableBuilding b = new SerializableBuilding
                    {
                        PlacedGameObject = go,
                        iD = id
                    };
                    placedBuildingGameObjects.Add(index,b);
                    
                }
                buf.Close();
                ModAPI.Console.Write("Loaded " + buildingCount);
            }
            catch (System.Exception EX)
            {
                ModAPI.Log.Write(EX.ToString());
            }

        }








        private static List<string> keywords = new List<string>();




        public struct SerializableBuilding
        {
            public int iD;
            public GameObject PlacedGameObject;
        }





        public static void Remove(GameObject go)
        {
            try
            {


                ;
                foreach (KeyValuePair<uint,SerializableBuilding> pair in placedBuildingGameObjects)
                {
                    if(pair.Value.PlacedGameObject == go)
                    {
                    placedBuildingGameObjects.Remove(pair.Key);
                    Object.Destroy(go);

                    }
                }
            }
            catch (System.Exception EX)
            {
                ModAPI.Log.Write(EX.ToString());
            }
        }


        public static GameObject Instantiate(int prefabID, Vector3 position)
        {
            GameObject prefab = prefabs[prefabID].gameObject;
            prefab.SetActive(true);
            GameObject gameObject = Object.Instantiate(prefab);
            gameObject.transform.position = Vector3.zero + position;

            SerializableBuilding serializableBuilding = new SerializableBuilding()
            {
                iD = prefabID,
                PlacedGameObject = gameObject
            };
            while (placedBuildingGameObjects.ContainsKey(LastIndex))
            {
                LastIndex++;
            }
            placedBuildingGameObjects.Add(LastIndex,serializableBuilding);
                LastIndex++;
            prefab.SetActive(false);

            return gameObject;
        }
        public static GameObject Instantiate(int prefabID, Vector3 position, out uint Index)
        {
            GameObject prefab = prefabs[prefabID].gameObject;
            prefab.SetActive(true);
            GameObject gameObject = Object.Instantiate(prefab);
            gameObject.transform.position = Vector3.zero + position;

            SerializableBuilding serializableBuilding = new SerializableBuilding()
            {
                iD = prefabID,
                PlacedGameObject = gameObject
            };
            while (placedBuildingGameObjects.ContainsKey(LastIndex))
            {
                LastIndex++;
            }
            placedBuildingGameObjects.Add(LastIndex, serializableBuilding);
            LastIndex++;
            prefab.SetActive(false);
            Index = LastIndex;
            return gameObject;
        }

        //defines the lists
        [ExecuteOnGameStart]
        public static void Initialize()
        {

            buildingData = new Dictionary<int, Building>();
            prefabs = new Dictionary<int, GameObject>();
            placedBuildingGameObjects = new Dictionary<uint, SerializableBuilding>();


        }



        //Called from outside mods, adds the prefab and building
        //Then they are added to the menu and can be instantiated
        public static void AddBuilding(Building building, int ID)
        {

            Building b = building;
            if (!prefabs.ContainsKey(ID))
            {
                GameObject go = CreatePefab(building);
                prefabs.Add(ID, go);
                go.SetActive(false);
            }
            else
            {
                ModAPI.Log.Write("Cannot add prefab with ID " + ID + ". It already is added!");
            }
            if (!buildingData.ContainsKey(ID))
            {
                buildingData.Add(ID, b);
            }
            else
            {
                ModAPI.Log.Write("Cannot add building with ID " + ID + ". It already is added!");

            }

        }

        //creates a prefab from data inside of a building struct
        public static GameObject CreatePefab(Building b)
        {
            try
            {
                bool MultipeMeshes = false;
                if (b.data.Length > 1)
                {
                    MultipeMeshes = true;
                }

                if (MultipeMeshes)
                {
                    GameObject parent = new GameObject("Building");

                    Shader s = Shader.Find("Standard");

                    for (int i = 0; i < b.data.Length; i++)
                    {
                        keywords.Clear();
                        Material material = new Material(s);
                        if (b.data[i].MainColor != null)
                            material.SetColor("_Color", b.data[i].MainColor);
                        if (b.data[i].MainTexture != null)
                            material.SetTexture("_MainTex", b.data[i].MainTexture);

                        if (b.data[i].BumpMap != null)
                        {
                            material.SetTexture("_BumpMap", b.data[i].BumpMap);
                            keywords.Add("_NORMALMAP");
                        }
                        material.SetFloat("_BumpScale", b.data[i].BumpScale);
                        if (b.data[i].HeightMap != null)
                        {
                            material.SetTexture("_ParallaxMap", b.data[i].HeightMap);
                            keywords.Add("_PARALLAXMAP");

                        }
                        material.SetFloat("_Parallax", b.data[i].HeightAmount);
                        material.SetFloat("_Glossiness", b.data[i].Smoothness);
                        if (b.data[i].MetalicTexture != null)
                        {
                            material.SetTexture("_MetallicGlossMap", b.data[i].MetalicTexture);
                            keywords.Add("_METALLICGLOSSMAP");
                        }
                        material.SetFloat("_Metallic", b.data[i].Metalic);
                        if (b.data[i].Occlusion != null)
                            material.SetTexture("_OcclusionMap", b.data[i].Occlusion);
                        material.SetFloat("_OcclusionStrength", b.data[i].OcclusionStrenght);
                        if (b.data[i].EmissionMap != null)
                        {
                            material.SetTexture("_EmissionMap", b.data[i].EmissionMap);
                            keywords.Add("_EMISSION");

                        }
                        if (b.data[i].EmissionColor != null)
                        {
                            keywords.Add("_EMISSION");
                            material.SetColor("_EmissionColor", b.data[i].EmissionColor);
                        }
                        if (b.data[i].renderMode == BuildingData.RenderMode.Opaque)
                        {
                            material.SetFloat("_Mode", 0);
                            material.SetInt("_ZWrite", 1);
                            material.SetInt("_SrcBlend", 1);
                            material.SetInt("_DstBlend", 0);
                            material.renderQueue = 2000;
                        }
                        else if (b.data[i].renderMode == BuildingData.RenderMode.Cutout)
                        {
                            material.SetFloat("_Mode", 1);
                            keywords.Add("_ALPHATEST_ON");
                            material.SetInt("_ZWrite", 1);
                            material.SetInt("_SrcBlend", 1);
                            material.SetInt("_DstBlend", 0);
                            material.renderQueue = 2450;

                        }
                        else if (b.data[i].renderMode == BuildingData.RenderMode.Fade)
                        {
                            material.SetFloat("_Mode", 2);
                            keywords.Add("_ALPHABLEND_ON");
                            material.SetInt("_ZWrite", 0);
                            material.SetInt("_SrcBlend", 5);
                            material.SetInt("_DstBlend", 10);
                            material.renderQueue = 3000;

                        }
                        else if (b.data[i].renderMode == BuildingData.RenderMode.Transparent)
                        {
                            material.SetFloat("_Mode", 3);
                            keywords.Add("_ALPHAPREMULTIPLY_ON");
                            material.SetInt("_ZWrite", 0);
                            material.SetInt("_SrcBlend", 1);
                            material.SetInt("_DstBlend", 10);
                            material.renderQueue = 3000;

                        }
                        material.shaderKeywords = keywords.ToArray();


                        GameObject o = new GameObject("BuilderCoreObject");

                        o.AddComponent<MeshFilter>().mesh = b.data[i].mesh;

                        o.AddComponent<Renderer>().material = material;

                        o.transform.parent = parent.transform;
                        o.transform.localScale = b.data[i].scale;
                        o.transform.localPosition = b.data[i].offset;
                      
                        if (b.data[i].AddCollider)
                        {
                         MeshCollider col =   o.AddComponent<MeshCollider>();
                            col.convex = b.data[i].Convex;
                            col.sharedMesh = b.data[i].mesh;
                        }
                        for (int a = 0; a < b.data[i].componets.Length; a++)
                        {
                            o.AddComponent(b.data[i].componets[a]);
                        }
                    }
                    return parent;
                }
                else
                {
                    keywords.Clear();
                    Material material = new Material(Shader.Find("Standard"));
                    int i = 0;
                    if (b.data[i].MainColor != null)
                        material.SetColor("_Color", b.data[i].MainColor);
                    if (b.data[i].MainTexture != null)
                        material.SetTexture("_MainTex", b.data[i].MainTexture);

                    if (b.data[i].BumpMap != null)
                    {
                        material.SetTexture("_BumpMap", b.data[i].BumpMap);
                        keywords.Add("_NORMALMAP");
                    }
                    material.SetFloat("_BumpScale", b.data[i].BumpScale);
                    if (b.data[i].HeightMap != null)
                    {
                        material.SetTexture("_ParallaxMap", b.data[i].HeightMap);
                        keywords.Add("_PARALLAXMAP");

                    }
                    material.SetFloat("_Parallax", b.data[i].HeightAmount);
                    if (b.data[i].MetalicTexture != null)
                    {
                        material.SetTexture("_MetallicGlossMap", b.data[i].MetalicTexture);
                        keywords.Add("_METALLICGLOSSMAP");
                    }
                    material.SetFloat("_Glossiness", b.data[i].Smoothness);
                    material.SetFloat("_Metallic", b.data[i].Metalic);
                    if (b.data[i].Occlusion != null)
                        material.SetTexture("_OcclusionMap", b.data[i].Occlusion);
                    material.SetFloat("_OcclusionStrength", b.data[i].OcclusionStrenght);
                    if (b.data[i].EmissionMap != null)
                    {
                        material.SetTexture("_Emission", b.data[i].EmissionMap);
                        keywords.Add("_EMISSION");

                    }
                    if (b.data[i].EmissionColor != null)
                    {
                        keywords.Add("_EMISSION");
                        material.SetColor("_EmissionColor", b.data[i].EmissionColor);
                    }
                    if (b.data[i].renderMode == BuildingData.RenderMode.Opaque)
                    {
                        material.SetFloat("_Mode", 0);
                        material.SetInt("_ZWrite", 1);
                        material.SetInt("_SrcBlend", 1);
                        material.SetInt("_DstBlend", 0);
                        material.renderQueue = 2000;
                    }
                    else if (b.data[i].renderMode == BuildingData.RenderMode.Cutout)
                    {
                        material.SetFloat("_Mode", 1);
                        keywords.Add("_ALPHATEST_ON");
                        material.SetInt("_ZWrite", 1);
                        material.SetInt("_SrcBlend", 1);
                        material.SetInt("_DstBlend", 0);
                        material.renderQueue = 2450;

                    }
                    else if (b.data[i].renderMode == BuildingData.RenderMode.Fade)
                    {
                        material.SetFloat("_Mode", 2);
                        keywords.Add("_ALPHABLEND_ON");
                        material.SetInt("_ZWrite", 0);
                        material.SetInt("_SrcBlend", 5);
                        material.SetInt("_DstBlend", 10);
                        material.renderQueue = 3000;

                    }
                    else if (b.data[i].renderMode == BuildingData.RenderMode.Transparent)
                    {
                        material.SetFloat("_Mode", 3);
                        keywords.Add("_ALPHAPREMULTIPLY_ON");
                        material.SetInt("_ZWrite", 0);
                        material.SetInt("_SrcBlend", 1);
                        material.SetInt("_DstBlend", 10);
                        material.renderQueue = 3000;

                    }
                    material.shaderKeywords = keywords.ToArray();
                    ////////////////////////////////////////////
                    GameObject o = new GameObject("BuilderCoreObject");

                    o.AddComponent<MeshFilter>().mesh = b.data[i].mesh;

                    o.AddComponent<Renderer>().material = material;

                    o.transform.localScale = b.data[i].scale;
                    o.transform.localPosition = b.data[i].offset;

                    if (b.data[i].AddCollider)
                    {
                        MeshCollider col = o.AddComponent<MeshCollider>();
                        col.convex = b.data[i].Convex;
                        col.sharedMesh = b.data[i].mesh;
                    }
                    for (int a = 0; a < b.data[i].componets.Length; a++)
                    {
                        o.AddComponent(b.data[i].componets[a]);
                    }
                    return o;
                }
            }
            catch (System.Exception EX)
            {
                ModAPI.Log.Write(EX.ToString());
            }
            return null;
        }
        public static Mesh ReadMeshFromOBJ(string Filepath)
        {
            MeshImporter importer = new MeshImporter();
           return importer.ImportFile(Filepath);
        }
        public static Mesh ReadMesh(string Filepath)
        {
            return ReadMeshFromBytes(File.ReadAllBytes(Filepath));
        }
        public static Material CreateMaterial(BuildingData data)
        {
            keywords.Clear();
            Material material = new Material(Shader.Find("Standard"));
            int i = 0;
            if (data.MainColor != null)
                material.SetColor("_Color", data.MainColor);
            if (data.MainTexture != null)
                material.SetTexture("_MainTex", data.MainTexture);

            if (data.BumpMap != null)
            {
                material.SetTexture("_BumpMap", data.BumpMap);
                keywords.Add("_NORMALMAP");
            }
            material.SetFloat("_BumpScale", data.BumpScale);
            if (data.HeightMap != null)
            {
                material.SetTexture("_ParallaxMap", data.HeightMap);
                keywords.Add("_PARALLAXMAP");

            }
            material.SetFloat("_Parallax", data.HeightAmount);
            material.SetFloat("_Glossiness", data.Smoothness);
            if (data.MetalicTexture != null)
            {
                material.SetTexture("_MetallicGlossMap", data.MetalicTexture);
                keywords.Add("_METALLICGLOSSMAP");
            }
            material.SetFloat("_Metallic", data.Metalic);
            if (data.Occlusion != null)
                material.SetTexture("_OcclusionMap", data.Occlusion);
            material.SetFloat("_OcclusionStrength", data.OcclusionStrenght);
            if (data.EmissionMap != null)
            {
                material.SetTexture("_Emission", data.EmissionMap);
                keywords.Add("_EMISSION");

            }
            if (data.EmissionColor != null)
            {
                keywords.Add("_EMISSION");
                material.SetColor("_EmissionColor", data.EmissionColor);
            }
            if (data.renderMode == BuildingData.RenderMode.Opaque)
            {
                material.SetFloat("_Mode", 0);
                material.SetInt("_ZWrite", 1);
                material.SetInt("_SrcBlend", 1);
                material.SetInt("_DstBlend", 0);
                material.renderQueue = 2000;
            }
            else if (data.renderMode == BuildingData.RenderMode.Cutout)
            {
                material.SetFloat("_Mode", 1);
                keywords.Add("_ALPHATEST_ON");
                material.SetInt("_ZWrite", 1);
                material.SetInt("_SrcBlend", 1);
                material.SetInt("_DstBlend", 0);
                material.renderQueue = 2450;

            }
            else if (data.renderMode == BuildingData.RenderMode.Fade)
            {
                material.SetFloat("_Mode", 2);
                keywords.Add("_ALPHABLEND_ON");
                material.SetInt("_ZWrite", 0);
                material.SetInt("_SrcBlend", 5);
                material.SetInt("_DstBlend", 10);
                material.renderQueue = 3000;

            }
            else if (data.renderMode == BuildingData.RenderMode.Transparent)
            {
                material.SetFloat("_Mode", 3);
                keywords.Add("_ALPHAPREMULTIPLY_ON");
                material.SetInt("_ZWrite", 0);
                material.SetInt("_SrcBlend", 1);
                material.SetInt("_DstBlend", 10);
                material.renderQueue = 3000;

            }
            material.shaderKeywords = keywords.ToArray();
            return material;
        }
        private static Mesh ReadMeshFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 12)
            {
                return null;
            }

            var buf = new BinaryReader(new MemoryStream(bytes));

            // read header
            int vertCount = buf.ReadInt32();
            int triCount = buf.ReadInt32();
            int format = buf.ReadInt32();


            // sanity check
            if (vertCount < 0 || vertCount > 65535)
            {
                return null;
            }
            if (triCount < 0 || triCount > 65535)
            {
                return null;
            }
            if (format < 1 || format > 15)
            {
                return null;
            }

            Mesh mesh = new Mesh();
            int i;

            // positions
            var verts = new Vector3[vertCount];
            for (i = 0; i < vertCount; ++i)
            {
                verts[i] = new Vector3(buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle());
            }
            mesh.vertices = verts;

            if (format >= 2) // have normals
            {
                Vector3[] normals = new Vector3[vertCount];
                for (i = 0; i < vertCount; ++i)
                {
                    normals[i] = new Vector3(buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle());
                }
                mesh.normals = normals;

            }

            if (format >= 4) // have tangents
            {
                Vector4[] tangents = new Vector4[vertCount];
                for (i = 0; i < vertCount; ++i)
                {
                    tangents[i] = new Vector4(buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle());
                }
                mesh.tangents = tangents;

            }

            if (format >= 8) // have UVs
            {
                Vector2[] uvs = new Vector2[vertCount];
                for (i = 0; i < vertCount; ++i)
                {
                    uvs[i] = new Vector2(buf.ReadSingle(), buf.ReadSingle());
                }
                mesh.uv = uvs;

            }
            // triangle indices
            var tris = new int[triCount * 3];
            for (i = 0; i < triCount; ++i)
            {
                tris[i * 3 + 0] = buf.ReadInt32();
                tris[i * 3 + 1] = buf.ReadInt32();
                tris[i * 3 + 2] = buf.ReadInt32();
            }
            mesh.triangles = tris;

            buf.Close();

            return mesh;
        }


        private static void AssignMaterialOnSkinnedMesh(ref GameObject gameObject, Building b)
        {
            SkinnedMeshRenderer rend = gameObject.GetComponent<SkinnedMeshRenderer>();
            if (rend == null)
            {
                ModAPI.Log.Write("Couldnt find skinned mesh renderer");
                return;
            }
            else
            {
                keywords.Clear();
                Material material = new Material(Shader.Find("Standard"));
                int i = 0;
                if (b.data[i].MainColor != null)
                    material.SetColor("_Color", b.data[i].MainColor);
                if (b.data[i].MainTexture != null)
                    material.SetTexture("_MainTex", b.data[i].MainTexture);
                
                if (b.data[i].BumpMap != null)
                {
                    material.SetTexture("_BumpMap", b.data[i].BumpMap);
                    keywords.Add("_NORMALMAP");
                }
                material.SetFloat("_BumpScale", b.data[i].BumpScale);
                if (b.data[i].HeightMap != null)
                {
                    material.SetTexture("_ParallaxMap", b.data[i].HeightMap);
                    keywords.Add("_PARALLAXMAP");

                }
                material.SetFloat("_Parallax", b.data[i].HeightAmount);
                material.SetFloat("_Glossiness", b.data[i].Smoothness);
                if (b.data[i].MetalicTexture != null)
                {
                    material.SetTexture("_MetallicGlossMap", b.data[i].MetalicTexture);
                    keywords.Add("_METALLICGLOSSMAP");
                }
                material.SetFloat("_Metallic", b.data[i].Metalic);
                if (b.data[i].Occlusion != null)
                    material.SetTexture("_OcclusionMap", b.data[i].Occlusion);
                material.SetFloat("_OcclusionStrength", b.data[i].OcclusionStrenght);
                if (b.data[i].EmissionMap != null)
                {
                    material.SetTexture("_Emission", b.data[i].EmissionMap);
                    keywords.Add("_EMISSION");

                }
                if (b.data[i].EmissionColor != null)
                {
                    keywords.Add("_EMISSION");
                    material.SetColor("_EmissionColor", b.data[i].EmissionColor);
                }
                if (b.data[i].renderMode == BuildingData.RenderMode.Opaque)
                {
                    material.SetFloat("_Mode", 0);
                    material.SetInt("_ZWrite", 1);
                    material.SetInt("_SrcBlend", 1);
                    material.SetInt("_DstBlend", 0);
                    material.renderQueue = 2000;
                }
                else if (b.data[i].renderMode == BuildingData.RenderMode.Cutout)
                {
                    material.SetFloat("_Mode", 1);
                    keywords.Add("_ALPHATEST_ON");
                    material.SetInt("_ZWrite", 1);
                    material.SetInt("_SrcBlend", 1);
                    material.SetInt("_DstBlend", 0);
                    material.renderQueue = 2450;

                }
                else if (b.data[i].renderMode == BuildingData.RenderMode.Fade)
                {
                    material.SetFloat("_Mode", 2);
                    keywords.Add("_ALPHABLEND_ON");
                    material.SetInt("_ZWrite", 0);
                    material.SetInt("_SrcBlend", 5);
                    material.SetInt("_DstBlend", 10);
                    material.renderQueue = 3000;

                }
                else if (b.data[i].renderMode == BuildingData.RenderMode.Transparent)
                {
                    material.SetFloat("_Mode", 3);
                    keywords.Add("_ALPHAPREMULTIPLY_ON");
                    material.SetInt("_ZWrite", 0);
                    material.SetInt("_SrcBlend", 1);
                    material.SetInt("_DstBlend", 10);
                    material.renderQueue = 3000;

                }
                material.shaderKeywords = keywords.ToArray();
                rend.sharedMaterial = material;
            }
        }
        public static void AddBuilding(Building b,int ID,string RiggedMeshFilepath)
        {
          
            if (!prefabs.ContainsKey(ID))
            {
                GameObject go = ReadMesh(File.ReadAllBytes(RiggedMeshFilepath));
                AssignMaterialOnSkinnedMesh(ref go, b);
                prefabs.Add(ID, go);
                go.SetActive(false);
            }
            else
            {
                ModAPI.Log.Write("Cannot add prefab with ID " + ID + ". It already is added!");
            }
            if (!buildingData.ContainsKey(ID))
            {
                buildingData.Add(ID, b);
            }
            else
            {
                ModAPI.Log.Write("Cannot add building with ID " + ID + ". It already is added!");

            }
        }

        private static GameObject ReadMesh(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 12)
            {
                return null;
            }

            var buf = new BinaryReader(new MemoryStream(bytes));

            // read header
            int vertCount = buf.ReadInt32();
            int triCount = buf.ReadInt32();
            int bonesCount = buf.ReadInt32();
            int weightCount = buf.ReadInt32();
            int format = buf.ReadInt32();


            // sanity check
            if (vertCount < 0 || vertCount > 65535)
            {
                return null;
            }
            if (triCount < 0 || triCount > 65535)
            {
                return null;
            }
            if (format < 1 || format > 15)
            {
                return null;
            }

            Mesh mesh = new Mesh();
            int i;

            // positions
            var verts = new Vector3[vertCount];
            for (i = 0; i < vertCount; ++i)
            {
                verts[i] = new Vector3(buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle());
            }
            mesh.vertices = verts;

            if (format >= 2) // have normals
            {
                Vector3[] normals = new Vector3[vertCount];
                for (i = 0; i < vertCount; ++i)
                {
                    normals[i] = new Vector3(buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle());
                }
                mesh.normals = normals;

            }

            if (format >= 4) // have tangents
            {
                Vector4[] tangents = new Vector4[vertCount];
                for (i = 0; i < vertCount; ++i)
                {
                    tangents[i] = new Vector4(buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle());
                }
                mesh.tangents = tangents;

            }

            if (format >= 8) // have UVs
            {
                Vector2[] uvs = new Vector2[vertCount];
                for (i = 0; i < vertCount; ++i)
                {
                    uvs[i] = new Vector2(buf.ReadSingle(), buf.ReadSingle());
                }
                mesh.uv = uvs;

            }
            // triangle indices
            var tris = new int[triCount * 3];
            for (i = 0; i < triCount; ++i)
            {
                tris[i * 3 + 0] = buf.ReadInt32();
                tris[i * 3 + 1] = buf.ReadInt32();
                tris[i * 3 + 2] = buf.ReadInt32();
            }
            mesh.triangles = tris;
            Transform[] bones = new Transform[bonesCount];
            GameObject root = null;
            GameObject parent = null;
            if (format >= 16) // have UVs
            {
                int[] parentIndexes = new int[bonesCount];
                for (i = 0; i < bonesCount; i++)
                {
                    int a = buf.ReadInt32();
                    string name = buf.ReadString();
                    Vector3 pos = new Vector3(buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle());
                    Quaternion rot = new Quaternion(buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle());
                    if (a == -1)
                    {
                        string parentName = buf.ReadString();
                        Vector3 parentpos = new Vector3(buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle());
                        Quaternion parentrot = new Quaternion(buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle());

                        string rootName = buf.ReadString();
                        Vector3 rootpos = new Vector3(buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle());
                        Quaternion rootrot = new Quaternion(buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle(), buf.ReadSingle());

                        root = new GameObject(rootName);
                        root.transform.position = rootpos;
                        root.transform.rotation = rootrot;

                        parent = new GameObject(parentName);
                        parent.transform.position = parentpos;
                        parent.transform.rotation = parentrot;
                        parent.transform.parent = root.transform;

                        GameObject obj = new GameObject(name);
                        obj.transform.position = pos;
                        obj.transform.rotation = rot;
                        bones[i] = obj.transform;

                    }
                    else
                    {
                        GameObject obj = new GameObject(name);
                        obj.transform.position = pos;
                        obj.transform.rotation = rot;
                        bones[i] = obj.transform;
                    }
                    parentIndexes[i] = a;
                }
                for (i = 0; i < bonesCount; i++)
                {
                    int index = parentIndexes[i];
                    if (index == -1)
                    {
                        bones[i].transform.parent = parent.transform;
                    }
                    else
                    {
                        bones[i].transform.parent = bones[index].transform;

                    }
                }

            }
            BoneWeight[] weights = new BoneWeight[weightCount];
            for (int x = 0; x < weightCount; x++)
            {
                BoneWeight w = new BoneWeight()
                {
                    boneIndex0 = buf.ReadInt32(),
                    boneIndex1 = buf.ReadInt32(),
                    boneIndex2 = buf.ReadInt32(),
                    boneIndex3 = buf.ReadInt32(),
                    weight0 = buf.ReadSingle(),
                    weight1 = buf.ReadSingle(),
                    weight2 = buf.ReadSingle(),
                    weight3 = buf.ReadSingle(),

                };
                weights[x] = w;
            }
            mesh.boneWeights = weights;
            SkinnedMeshRenderer rend = root.AddComponent<SkinnedMeshRenderer>();
            rend.bones = bones;
            rend.sharedMesh = mesh;

            buf.Close();

            return root;
        }
    }
    public class Building
    {
        public Building()
        {
            data = new BuildingData[]
            {
                new BuildingData()

            };
            Width = 1;
            Height = 1;
            Lenght = 1;
            save = true;
        }

        public BuildingData[] data;

        //Height Width and Lenght must be defined, used for snapping
        public float Width;
        public float Height;
        public float Lenght;
        public bool save;

    }
    public class BuildingData
    {
        public enum RenderMode
        {
            Opaque,
            Cutout,
            Fade,
            Transparent
        }
        public BuildingData()
        {
            mesh = null;
            MainTexture = null;
            MainColor = Color.white;
            BumpMap = null;
            BumpScale = 1;
            HeightAmount = 0.02f;
            HeightMap = null;
            Occlusion = null;
            OcclusionStrenght = 1;
            EmissionColor = Color.black;
            EmissionMap = null;
            Metalic = 0;
            Smoothness = 0.5f;
            scale = Vector3.one;
            offset = Vector3.zero;
            AddCollider = true;
            componets = new System.Type[0];
            Convex = false;
            renderMode = RenderMode.Opaque;
            MetalicTexture = null;

        }
        //mesh of the object obtained from ReadMesh
        public Mesh mesh;

        //material of the object. You can use ModAPI.Resources to get textures. Beware that modapi doesnt support high res textures;
        public Texture2D MainTexture;
        public Color MainColor;

        public Texture2D BumpMap;
        public float BumpScale;

        public Texture2D HeightMap;
        public float HeightAmount;

        public Texture2D Occlusion;
        public float OcclusionStrenght;

        public Texture2D EmissionMap;
        public Color EmissionColor;

        public float Metalic;
        public Texture2D MetalicTexture;
        public float Smoothness;
        //Scale and offset of the individual mesh
        public Vector3 scale;
        public Vector3 offset;

        //This bool determines if the mesh should have collision.
        //If true add a mesh collider
        public bool AddCollider;

        //Array of all component types that should be added to the object
        public System.Type[] componets;

        //Should the mesh collider use convex
        public bool Convex;

        public RenderMode renderMode;
    }
}
