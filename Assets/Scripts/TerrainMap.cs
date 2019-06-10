/*
    Copyright 2016 Esri

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

using Academy.HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Networking;
using Esri.PrototypeLab.HoloLens.Unity;
using UnityEngine.UI;


using UnityEngine.Windows.Speech;
using SimpleJSON;

namespace Esri.HoloLens.APP {
    public class TerrainMap : MonoBehaviour
    {
        [Tooltip("Minimum distance from user")]
        public float MinimumDistance = 0.5f;

        [Tooltip("Maximum distance from user")]
        public float MaximimDistance = 10f;

        public StaticMenu buttonZoomPrefab;
        //public StableMenu buttonChoosePrefab;
        //public StableMenu buttonCallPrefab;
        private float rotationFactorY;
        private Vector3 navigationPreviousPosition;

        private UnityEngine.XR.WSA.Input.GestureRecognizer _gestureRecognizer = null;
        private KeywordRecognizer _keywordRecognizer = null;
        private bool _isMoving = true;
        private bool _NeedReloadKeywords = false;
        private bool _isMapLoaded = false;
        private bool _isKeywordsLoaded = false;
        private bool _isFirstTimeLoading = false;
        private bool _NeedReloadMap = false;
        public Place _place;
        private Place[] places;
        private Keyword[] keywordslist;

        private const float HIT_OFFSET = 0.01f;
        private const float HOVER_OFFSET = 2f;
        public float RotationSensitivity = 25.0f;
        public float SIZE = 1f;
        private const float HEIGHT = 1f;
        private const string SPEECH_PREFIX = "show";
        private const string SPEECH_PREFIX_keywords = "find";
        private const float TERRAIN_BASE_HEIGHT = 0.02f;
        private const float VERTICAL_EXAGGERATION = 1.5f;
        public int CHILDREN_LEVEL = 2; // 1 = Four child image tiles, 2 = Sixteen child images.
        private string queryURL = "http://142.104.69.88:8080/querydata.php";
        private string updateURL = "http://142.104.69.88:8080/updatedata.php";
        private string mapName;
        private string reloadmapname;
        private string m_xml = "initial";
        private DateTime m_datetime;
        private StaticMenu buttonObject;
        public string currentDimension = "2D";
        public string currentStyle = "street";
        public Mark markPrefab;


        public void Start()
        {

            m_datetime = DateTime.Now;
            currentDimension = "2D";
            StartCoroutine(DownloadPlaces(queryURL));
            StartCoroutine(DownloadKeywords(queryURL));
            this._gestureRecognizer = new UnityEngine.XR.WSA.Input.GestureRecognizer();
            this._gestureRecognizer.SetRecognizableGestures(
                UnityEngine.XR.WSA.Input.GestureSettings.Tap |
                UnityEngine.XR.WSA.Input.GestureSettings.DoubleTap |
                UnityEngine.XR.WSA.Input.GestureSettings.Hold |
                UnityEngine.XR.WSA.Input.GestureSettings.ManipulationTranslate
            );

            /*
            this._gestureRecognizer.HoldStartedEvent += (source, ray) =>
            {
                
            };
            */
            


            this._gestureRecognizer.ManipulationStartedEvent += (source, cumulativeDelta, ray) => {
                navigationPreviousPosition = cumulativeDelta;
            };

            this._gestureRecognizer.ManipulationCompletedEvent += (source, cumulativeDelta, ray) => {
                
            };
            


            // Repond to single and double tap.
            this._gestureRecognizer.TappedEvent += (source, count, ray) =>
            {
                var terrain = this.transform.Find("terrain");
                switch (count)
                {
                    case 1:
                        if (this._isMoving)
                        {
                            //if (GazeManager.Instance.Hit)
                            {
                                // Cease moving
                                this._isMoving = false;

                                // Stop mapping observer
                                SpatialMappingManager.Instance.StopObserver();
                                
                                if (terrain == null)
                                {
                                    // Hide footprint
                                    this.transform.Find("base").GetComponent<MeshRenderer>().material.color = new Color32(100, 100, 100, 100);

                                    // Add Terrain
                                    _isFirstTimeLoading = true;
                                }
                                else
                                {
                                    // Restore hit test
                                    terrain.gameObject.layer = 0;
                                }
                            }
                        }
                        else
                        {
                            // If single tap on stationary terrain then perform reverse geocode.
                            // Exit if nothing found
                            if (!GazeManager.Instance.Hit) { return; }
                            if (GazeManager.Instance.FocusedObject == null) { return; }

                            // Exit if not terrain
                            if (GazeManager.Instance.FocusedObject.GetComponent<Terrain>() == null) { return; }

                            //build control button

                            if (buttonObject == null)
                            {
                                buttonObject = Instantiate(buttonZoomPrefab, terrain.transform.position + new Vector3(1f, 0.3f, 0.5f), Quaternion.Euler(60, 0, 0)) as StaticMenu;
                            }
                            
                            //buttonObject.transform.parent = terrain.transform;

                           //this.StartCoroutine(this.AddStreetAddress(GazeManager.Instance.Position));


                        }
                        break;
                    case 2:
                        // Resume footprint/terrain movement.
                        if (!this._isMoving)
                        {
                            // Set moving flag for update method
                            this._isMoving = true;

                            // Make terrain hittest invisible
                            this.GetComponentInChildren<Terrain>().gameObject.layer = 2;

                            // Resume mapping observer
                            SpatialMappingManager.Instance.StartObserver();
                        }
                        break;
                }
            };



            this._gestureRecognizer.StartCapturingGestures();

            // Create terrain footprint.
            
            var footprint = GameObject.CreatePrimitive(PrimitiveType.Quad);
            footprint.name = "base";
            footprint.transform.position = new Vector3(0, 0, 0);
            footprint.transform.localRotation = Quaternion.FromToRotation(
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1)
            );
            footprint.transform.localScale = new Vector3(SIZE, SIZE, 1f);
            footprint.layer = 2; // Ignore Raycast
            footprint.transform.parent = this.transform;
            
        }

        public void Update()
        {

            DateTime dTimeNow = DateTime.Now;
            TimeSpan ts = dTimeNow.Subtract(m_datetime);
            float tsf = float.Parse(ts.TotalSeconds.ToString());
            //每隔3s获取以下地图信息，以及哪张地图被加载
            if (tsf > 5)
            {
                StartCoroutine(DownloadPlaces(queryURL));
                //如果地图已经加载过则会查询是否有更新过的地图
                var terrain = this.transform.Find("terrain");
                if (terrain != null)
                {
                    StartCoroutine(CheckExistmap(queryURL));
                }
                m_datetime = DateTime.Now;

            }
            if (!this._isMoving) { return; }

            // Exit if Gaze Manager not created.
            if (GazeManager.Instance == null) { return; }

            // Reposition terra
            this.transform.position =
                GazeManager.Instance.Hit ?
                GazeManager.Instance.Position + TerrainMap.HIT_OFFSET * GazeManager.Instance.Normal :
                Camera.main.transform.position + TerrainMap.HOVER_OFFSET * Camera.main.transform.forward;

            // Update footprint color.
            this.transform.Find("base").GetComponent<MeshRenderer>().material.color =
                GazeManager.Instance.Hit ?
                new Color32(0, 255, 0, 100) :
                new Color32(255, 0, 0, 100);

            }
            //inital map when mapdata has been loaded

        public void LateUpdate()
        {

            if (this._isMapLoaded && this._isKeywordsLoaded && this._isFirstTimeLoading)
            {
                var terrain = this.transform.Find("terrain");
                if (terrain == null)
                {
                    for (int i = 0; i < places.Length; i++)
                    {
                        if (places[i].Name == "Default")
                        {
                            mapName = places[i].Name;
                            this._place = places[i];
                            this.StartCoroutine(this.AddTerrain(places[i]));
                        }
                    }
                }
                else
                {
                    // Restore hit test
                    terrain.gameObject.layer = 0;
                }

                this._isFirstTimeLoading = false;

                var names = places.Select(p =>
                {
                    return string.Format("{0} {1}", SPEECH_PREFIX, p.Name);
                });

                names = names.Concat(keywordslist.Select(p =>
                {
                    return string.Format("{0} {1}", SPEECH_PREFIX_keywords, p.keyword);
                }));

                this._keywordRecognizer = new KeywordRecognizer(names.ToArray());

                this._keywordRecognizer.OnPhraseRecognized += (e) =>
                {
                    // Exit if recognized speech not reliable.
                    if (e.confidence == ConfidenceLevel.Rejected) { return; }

                    if (e.text.Substring(0, SPEECH_PREFIX.Length).Equals(SPEECH_PREFIX))
                    {
                        //find map
                        string name = e.text.Substring(SPEECH_PREFIX.Length);
                        Place place = places.FirstOrDefault(p =>
                        {
                            return p.Name.ToLowerInvariant() == name.Trim().ToLowerInvariant();
                        });
                        if (place == null) { return; }
                        mapName = place.Name;
                        this.StartCoroutine(this.AddTerrain(place));
                    }
                    else if (e.text.Substring(0, SPEECH_PREFIX_keywords.Length).Equals(SPEECH_PREFIX_keywords))
                    {
                        //find keywords
                        string keywordname = e.text.Substring(SPEECH_PREFIX_keywords.Length);
                        Keyword keyword = keywordslist.FirstOrDefault(p =>
                        {
                            return p.keyword.ToLowerInvariant() == keywordname.Trim().ToLowerInvariant();
                        });

                        if (keyword.keyword.Equals("cherryblossom") && (this._place.Name.Equals("Victoria") || this._place.Name.Equals("Default")))
                        {
                            this.StartCoroutine(this.GetCherryBlossom());
                        }
                        else
                        {
                            this.StartCoroutine(this.GetSuggestion(this._place, keyword.keyword));
                        }

                        GameObject[] objs = GameObject.FindGameObjectsWithTag("suggestmark");

                        if (objs.Length > 0)
                        {
                            foreach (var obj in objs)
                            {
                                Destroy(obj);
                            }
                        }
                    }

                };
                this._keywordRecognizer.Start();

                this._NeedReloadKeywords = false;

                //create
            }
        
            //add 1
            if (this._NeedReloadMap && !this._isFirstTimeLoading)
            {
                for (int i = 0; i < places.Length; i++)
                {
                    if (places[i].Name == reloadmapname)
                    {
                        mapName = places[i].Name;
                        this._NeedReloadMap = false;
                        this.StartCoroutine(this.AddTerrain(places[i]));
                    }
                }
            }
        }

        private IEnumerator AddTerrain(Place place)
        {
            // Store current place
            this._place = place;

            // Convert lat/long to Google/Bing/AGOL tile.
            var tile = this._place.Location.ToTile(this._place.Level);

            // Get children.
            //var children = tile.GetChildren(CHILDREN_LEVEL);

            var children = tile.GetNeighbor(CHILDREN_LEVEL); 

            // Elevation and texture variables.
            ElevationData el = null;
            Texture2D[] textures = new Texture2D[children.Length];
            yield return null;

            // Retrieve elevation.
            
            if (currentDimension == "3D")
            {
                this.StartCoroutine(Elevation.GetHeights(tile, elevation =>
                {
                    el = elevation;
                    // Construct terrain if both elevation and textures downloaded.
                    if (textures.All(t => t != null))
                    {
                        this.StartCoroutine(this.BuildTerrain(el, textures));
                    }
                }));
                yield return null;
            }      

            // Retrieve imagery.
            foreach (var child in children)
            {
                this.StartCoroutine(Imagery.GetTexture(child, currentStyle, texture =>
                {
                    textures[Array.IndexOf(children, child)] = texture;
                    // Construct terrain if both elevation and textures downloaded.
                    //if (el != null && textures.All(t => t != null))
                    if (textures.All(t => t != null))
                    {
                        this.StartCoroutine(this.BuildTerrain(el, textures));
                    }
                }));
            }
			//add 2
            /*
            if (needupdate)
            {
                this.StartCoroutine(this.Updatemap(updateURL));
                //destroy all exist building，erase the items form
                GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Rotate");
                for (var i = 0; i < gameObjects.Length; i++)
                {
                    Destroy(gameObjects[i]);
                }
                //StartCoroutine(Eraseitems(queryURL));
            }
            */
            
        }

        private IEnumerator BuildTerrain(ElevationData elevation, Texture2D[] textures)
        {
            // the terraindate refresh very slow\
            /*
            if (currentDimension == "2D" && GameObject.Find("terrain") != null)
            {
                GameObject go = GameObject.Find("terrain");
                TerrainCollider tcr = (TerrainCollider)go.GetComponent<TerrainCollider>();
                Terrain trn = (Terrain)go.GetComponent<Terrain>();
                //do not need to rebuild terrain,just replace textures          
                // Splat maps.
                var dim = (int)Math.Sqrt(textures.Length);
                var splat = new List<SplatPrototype>();
                foreach (var texture in textures)
                {
                    splat.Add(new SplatPrototype()
                    {
                        tileOffset = new Vector2(0, 0),
                        tileSize = new Vector2(
                            SIZE / dim,
                            SIZE / dim
                        ),
                        texture = texture
                    });
                    yield return null;
                }

                trn.terrainData.splatPrototypes = splat.ToArray();
                yield return null;
                yield break;
            }
            */
            
            while (true)
            {
                GameObject tc = GameObject.Find("terrain");
                if (tc != null)
                {
                    GameObject.Destroy(tc);
                }
                else
                {
                    break;
                }
                yield return null;
            }
           
            // Center position of terrain.
            

            var position = this.transform.position;
            position -= new Vector3(SIZE / 2, 0, SIZE / 2);

            // Create terrain game object.
            GameObject terrainObject = new GameObject("terrain");
            terrainObject.transform.position = position;
            terrainObject.transform.parent = this.transform;
            yield return null;

            // Create terrain data.
            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = 33;
            terrainData.size = new Vector3(SIZE, HEIGHT, SIZE);
            terrainData.alphamapResolution = 32;
            terrainData.baseMapResolution = 1024;
            terrainData.SetDetailResolution(1024, 8);
            yield return null;

            // Tiles per side.
            var dimension = (int)Math.Sqrt(textures.Length);

            // Splat maps.
            var splats = new List<SplatPrototype>();
            foreach (var texture in textures)
            {
                splats.Add(new SplatPrototype()
                {
                    tileOffset = new Vector2(0, 0),
                    tileSize = new Vector2(
                        SIZE / dimension,
                        SIZE / dimension
                    ),
                    texture = texture
                });
                yield return null;
            }
            terrainData.splatPrototypes = splats.ToArray();
            terrainData.RefreshPrototypes();
            yield return null;

            // Get tile
            var tile = this._place.Location.ToTile(this._place.Level);

            // Construct height map.
            float[,] data = new float[
                terrainData.heightmapWidth,
                terrainData.heightmapHeight
            ];
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                for (int y = 0; y < terrainData.heightmapHeight; y++)
                {
                    // Scale elevation from 257x257 to 33x33
                    if (elevation != null)
                    {
                        var x2 = Convert.ToInt32((double)x * 256 / (terrainData.heightmapWidth - 1));
                        var y2 = Convert.ToInt32((double)y * 256 / (terrainData.heightmapHeight - 1));

                        // Find index in Esri elevation array
                        var id = y2 * 257 + x2;

                        // Absolute height in map units.
                        var h1 = elevation.Heights[id];

                        // Height in model units.                        
                        var h2 = SIZE * (h1 - elevation.Min) / tile.Size;

                        // Apply exaggeration.
                        var h3 = h2 * VERTICAL_EXAGGERATION;

                        // Apply base offset.                  
                        var h4 = h3 + TERRAIN_BASE_HEIGHT;
                        data[terrainData.heightmapHeight - 1 - y, x] = h4;
                    }                  
                    else
                    {
                        data[terrainData.heightmapHeight - 1 - y, x] = TERRAIN_BASE_HEIGHT;
                    }

                    // Final height.                   
                    
                }
            }
            terrainData.SetHeights(0, 0, data);
            yield return null;

            // Add alpha mapping
            //float[,,] maps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            var maps = new float[
                terrainData.alphamapWidth,
                terrainData.alphamapHeight,
                textures.Length
            ];
            for (int y = 0; y < terrainData.alphamapHeight; y++)
            {
                for (int x = 0; x < terrainData.alphamapWidth; x++)
                {
                    // Convert alpha coordinates into tile index. Left to right, bottom to top.
                    var tilex = x / (terrainData.alphamapWidth / dimension);
                    var tiley = y / (terrainData.alphamapHeight / dimension);
                    var index = (dimension - tiley - 1) * dimension + tilex;
                    for (int t = 0; t < textures.Length; t++)
                    {
                        maps[y, x, t] = index == t ? 1f : 0f;
                    }
                }
            }
            terrainData.SetAlphamaps(0, 0, maps);
            yield return null;

            
            // Create terrain collider.
            TerrainCollider terrainCollider = terrainObject.AddComponent<TerrainCollider>();
            terrainCollider.terrainData = terrainData;
            yield return null;

            // Add terrain component.
            Terrain terrain = terrainObject.AddComponent<Terrain>();
            terrain.terrainData = terrainData;
            yield return null;

            // Calculate mesh vertices and triangles.
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            // Distance between vertices
            var step = SIZE / 32f;

            // Front 
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                vertices.Add(new Vector3(x * step, 0f, 0f));
                vertices.Add(new Vector3(x * step, data[0, x], 0f));
            }
            yield return null;

            // Right
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                vertices.Add(new Vector3(SIZE, 0f, z * step));
                vertices.Add(new Vector3(SIZE, data[z, terrainData.heightmapWidth - 1], z * step));
            }
            yield return null;

            // Back
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                var xr = terrainData.heightmapWidth - 1 - x;
                vertices.Add(new Vector3(xr * step, 0f, SIZE));
                vertices.Add(new Vector3(xr * step, data[terrainData.heightmapHeight - 1, xr], SIZE));
            }
            yield return null;

            // Left
            for (int z = 0; z < terrainData.heightmapHeight; z++)
            {
                var zr = terrainData.heightmapHeight - 1 - z;
                vertices.Add(new Vector3(0f, 0f, zr * step));
                vertices.Add(new Vector3(0f, data[zr, 0], zr * step));
            }
            yield return null;

            // Quads
            for (int i = 0; i < vertices.Count / 2 - 1; i++)
            {
                triangles.AddRange(new int[] {
                    2 * i + 0,
                    2 * i + 1,
                    2 * i + 2,
                    2 * i + 2,
                    2 * i + 1,
                    2 * i + 3
                });
            }

            // Create single mesh for all four sides
            GameObject side = new GameObject("side");
            side.transform.position = position;
            side.transform.parent = terrainObject.transform;
            yield return null;

            // Create mesh
            Mesh mesh = new Mesh()
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            yield return null;


            MeshFilter meshFilter = side.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            yield return null;

            MeshRenderer meshRenderer = side.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"))
            {
               color = new Color32(0, 128, 128, 100)
            };
            yield return null;

            MeshCollider meshCollider = side.AddComponent<MeshCollider>();
            yield return null;

        }

        

        private IEnumerator ChangeMapCoordinates(Vector3 position)
        {
            // Get UL and LR coordinates
            var tileUL = this._place.Location.ToTile(this._place.Level);
            var tileLR = new Tile()
            {
                Zoom = tileUL.Zoom,
                X = tileUL.X + CHILDREN_LEVEL * 2,
                Y = tileUL.Y + CHILDREN_LEVEL * 2
            };
            var coordUL = tileUL.UpperLeft(CHILDREN_LEVEL);
            var coordLR = tileLR.UpperLeft(CHILDREN_LEVEL);

            // Get tapped location relative to lower left.
            GameObject terrain = GameObject.Find("terrain");
            var location = position - terrain.transform.position;

            var longitude = coordUL.Longitude + (coordLR.Longitude - coordUL.Longitude) * (location.x / SIZE);
            var lattitude = coordLR.Latitude + (coordUL.Latitude - coordLR.Latitude) * (location.z / SIZE);

            var coordinate = new Coordinate()
            {
                Longitude = longitude,
                Latitude = lattitude
            };
            this._place.Location = coordinate;
            //this._place.Level += 1;
            this.StartCoroutine(this.AddTerrain(this._place));

            yield return null;
        }

        private IEnumerator AddStreetAddress(Vector3 position)
        {
            // Get UL and LR coordinates
            var tileUL = this._place.Location.ToTile(this._place.Level);
            var tileLR = new Tile()
            {
                Zoom = tileUL.Zoom,
                X = tileUL.X + CHILDREN_LEVEL * 2,
                Y = tileUL.Y + CHILDREN_LEVEL * 2
            };
            var coordUL = tileUL.UpperLeft(CHILDREN_LEVEL);
            var coordLR = tileLR.UpperLeft(CHILDREN_LEVEL);

            // Get tapped location relative to lower left.
            GameObject terrain = GameObject.Find("terrain");
            var location = position - terrain.transform.position;

            var longitude = coordUL.Longitude + (coordLR.Longitude - coordUL.Longitude) * (location.x / SIZE);
            var lattitude = coordLR.Latitude + (coordUL.Latitude - coordLR.Latitude) * (location.z / SIZE);
            var coordinate = new Coordinate()
            {
                Longitude = longitude,
                Latitude = lattitude
            };

        // Retrieve address.
            this.StartCoroutine(GeocodeServer.ReverseGeocode(coordinate, address => {
                // Exit if no address found.
                if (address == null)
                {
                    System.Diagnostics.Debug.WriteLine("No Address");
                    return;
                }

                // Create leader line.
                GameObject line = new GameObject();
                line.transform.parent = terrain.transform;
                line.tag = "addressmark";

                LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Standard"))
                {
                    color = Color.white
                };
                lineRenderer.SetWidth(0.002f, 0.002f);
                lineRenderer.SetVertexCount(2);
                lineRenderer.SetPositions(new Vector3[] {
                        position,
                        position + Vector3.up * 0.15f
                });
                lineRenderer.receiveShadows = false;
                lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
                lineRenderer.useWorldSpace = false;

                // Add text
                GameObject text = new GameObject();
                text.transform.parent = terrain.transform;
                text.tag = "addressmark";
                text.transform.position = position + Vector3.up * 0.15f;
                text.transform.localScale = new Vector3(0.002f, 0.002f, 1f);

                TextMesh textMesh = text.AddComponent<TextMesh>();
                textMesh.text = address.SingleLine;
                textMesh.anchor = TextAnchor.LowerCenter;
                textMesh.fontSize = 50;
                textMesh.richText = true;

                textMesh.color = Color.green;

                Billboard billboard = text.AddComponent<Billboard>();
                billboard.PivotAxis = PivotAxis.Y;
             }));
            yield return null;
        }

        private void CreatTagonMap(Coordinate location, string addresss, bool showText)
        {
            var tileUL = this._place.Location.ToTile(this._place.Level);
            var tileLR = new Tile()
            {
                Zoom = tileUL.Zoom,
                X = tileUL.X + CHILDREN_LEVEL * 2,
                Y = tileUL.Y + CHILDREN_LEVEL * 2
            };
            var coordUL = tileUL.UpperLeft(CHILDREN_LEVEL);
            var coordLR = tileLR.UpperLeft(CHILDREN_LEVEL);

            // Get tapped location relative to lower left.
            GameObject terrain = GameObject.Find("terrain");

            Vector3 locationonMap = new Vector3();

            locationonMap.x = (location.Longitude - coordUL.Longitude) / (coordLR.Longitude - coordUL.Longitude) * SIZE;
            locationonMap.z = (1 - (location.Latitude - coordUL.Latitude) / (coordLR.Latitude - coordUL.Latitude)) * SIZE;
            
            var positiononMap = locationonMap + terrain.transform.position;

            if ((positiononMap.x <= terrain.transform.position.x + SIZE ) && (positiononMap.x >= terrain.transform.position.x) &&
                (positiononMap.z <= terrain.transform.position.z + SIZE) && (positiononMap.z >= terrain.transform.position.z))
            {
                //creat a red dot on the map
                Mark markobj = Instantiate(markPrefab, positiononMap + Vector3.up * 0.03f, Quaternion.Euler(0, -90, 0)) as Mark;
                markobj.transform.parent = terrain.transform;
                markobj.transform.localScale = markobj.transform.localScale * 2;
                markobj.tag = "suggestmark";
                // Add text
                if (showText)
                {
                    GameObject text = new GameObject();
                    text.transform.parent = terrain.transform;
                    text.tag = "suggestmark";
                    //text.tag = "Address";
                    text.transform.position = positiononMap + Vector3.up * 0.15f;
                    text.transform.localScale = new Vector3(0.002f, 0.002f, 1f);

                    TextMesh textMesh = text.AddComponent<TextMesh>();
                    textMesh.text = addresss;
                    textMesh.anchor = TextAnchor.LowerCenter;
                    textMesh.fontSize = 50;
                    textMesh.richText = true;
                    textMesh.color = Color.yellow;

                    Billboard billboard = text.AddComponent<Billboard>();
                    billboard.PivotAxis = PivotAxis.Y;
                }
                else
                {
                    markobj.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.75f, 0.8f, 1f);
                }
                          
            }
        }

        private IEnumerator GetSuggestion(Place place, string keyword)
        {
            var coordinate = new Coordinate()
            {
                Longitude = place.Location.Longitude,
                Latitude = place.Location.Latitude,
            };

            // Retrieve address.
            this.StartCoroutine(GeocodeSuggestion.GetSuggestGeocode(keyword, coordinate, suggestAddresses =>
            {
                // Exit if no address found.
                if (suggestAddresses == null)
                {
                    System.Diagnostics.Debug.WriteLine("No Address");
                    return;
                }

                SuggestionLocation[] suggestionsLocation = new SuggestionLocation[suggestAddresses.Length];
                int i = 0;
                foreach (SuggestAddresses suggestion in suggestAddresses)
                {
                    if (suggestion != null)
                    {
                        this.StartCoroutine(SuggestionToGeocode.suggestionToGeocode(suggestion, locations =>
                        {
                            //
                            if (locations == null)
                            {
                                System.Diagnostics.Debug.WriteLine("No such Address");
                                return;
                            }

                            SuggestionLocation[] suggestlocation = new SuggestionLocation[locations.Length];
                            int j = 0;
                            foreach (SuggestionLocation location in locations)
                            {

                                suggestionsLocation[j] = new SuggestionLocation();
                                suggestionsLocation[j].Location = new Coordinate();
                                suggestionsLocation[j].address = location.address;
                                suggestionsLocation[j].Location = location.Location;
                                suggestionsLocation[j].score = location.score;

                                //every location creat flag on the map
                                CreatTagonMap(location.Location, location.address, true);

                            }
                            j++;
                        }));
                    }
                    i++;
                }
            }));

            yield return null;
        }

        private IEnumerator GetCherryBlossom()
        {
            // Retrieve address.
            this.StartCoroutine(GeocodeCherryBlossom.GetCherryBlossomLocation(locations =>
            {
                // Exit if no address found.
                if (locations == null)
                {
                    System.Diagnostics.Debug.WriteLine("No Address");
                    return;
                }

                foreach (CherryBlossomLocation location in locations)
                {
                    //every location creat flag on the map
                    int now = int.Parse(DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString());
                    if (location != null && int.Parse(location.StartDate) < now && int.Parse(location.EndDate) > now)
                    {
                        CreatTagonMap(location.Location, location.address, false);
                    }
                     
                }

            })); 

            yield return null;
        }

        private IEnumerator DownloadPlaces(string url)
        {
            string xml;
            int i = 0;
            url += "?action=maplist";
            UnityWebRequest hs_get = UnityWebRequest.Get(url);
            yield return hs_get.SendWebRequest();
            if (hs_get.error != "" && hs_get.error != null)
            {
                Debug.Log(hs_get.error);
                xml = "";
            }
            else
            {
                xml = hs_get.downloadHandler.text;
                if (!xml.Equals(m_xml) || (m_xml.Equals("initial")))
                {
                    if (xml.Equals(m_xml))
                    {
                        this._NeedReloadKeywords = false;
                    }
                    else
                    {
                        m_xml = xml;
                        this._NeedReloadKeywords = true;
                        string[] maps = xml.Split('\n');
                        this.places = new Place[maps.Length - 1];

                        foreach (string map in maps)
                        {
                            if (map.Length > 0)
                            {
                                string[] mapinfo = map.Split('\t');
                                places[i] = new Place();
                                places[i].Name = mapinfo[0];
                                places[i].Location = new Coordinate();
                                places[i].Location.Longitude = float.Parse(mapinfo[1]);
                                places[i].Location.Latitude = float.Parse(mapinfo[2]);
                                places[i].Level = int.Parse(mapinfo[3]);
                                i++;

                            }
                        }
                    }
                }
                this._isMapLoaded = true;
            }
        }

        private IEnumerator DownloadKeywords(string url)
        {
            string xml;
            int i = 0;
            url += "?action=keywords";
            UnityWebRequest hs_get = UnityWebRequest.Get(url);
            yield return hs_get.SendWebRequest();
            if (hs_get.error != "" && hs_get.error != null)
            {
                Debug.Log(hs_get.error);
                xml = "";
            }
            else
            {
                xml = hs_get.downloadHandler.text;

                string[] keywords = xml.Split('\n');
                this.keywordslist = new Keyword[keywords.Length - 1];

                foreach (string keyword in keywords)
                {
                    if (keyword.Length > 0)
                    {
                        keywordslist[i] = new Keyword();
                        keywordslist[i].keyword = keyword;
                        i++;
                    }
                }
                this._isKeywordsLoaded = true;
            }
        }

        public void OnClickZoomOut()
        {
            // 缩小 - 图标
            this._place.Level -= 1;
            this.StartCoroutine(this.AddTerrain(this._place)); 
        }

        public void OnClickZoomIn()
        {
            //zoom in 放大 + 图标
            this._place.Level += 1;
            this.StartCoroutine(this.AddTerrain(this._place));
        }

        public void OnClickMoveConfirm(Vector3 position)
        {
            this.StartCoroutine(this.ChangeMapCoordinates(position - new Vector3(0f, 0.2f, 0f)));
        }

        public void OnClickMoveLeft()
        {
            this._place = GetLocalTileCenterCoordintes("left");
            this.StartCoroutine(this.AddTerrain(this._place));
        }

        public void OnClickMoveRight()
        {
            this._place = GetLocalTileCenterCoordintes("right");
            this.StartCoroutine(this.AddTerrain(this._place));
        }

        public void OnClickMoveUp()
        {
            this._place = GetLocalTileCenterCoordintes("up");
            this.StartCoroutine(this.AddTerrain(this._place));
        }

        public void OnClickMoveDown()        
        {
            this._place = GetLocalTileCenterCoordintes("down");
            this.StartCoroutine(this.AddTerrain(this._place));
        }

        public void OnClickMoveLeftUp()
        {
            this._place = GetLocalTileCenterCoordintes("leftup");
            this.StartCoroutine(this.AddTerrain(this._place));
        }
        public void OnClickMoveLeftDown()
        {
            this._place = GetLocalTileCenterCoordintes("leftdown");
            this.StartCoroutine(this.AddTerrain(this._place));
        }
        public void OnClickMoveRightUp()
        {
            this._place = GetLocalTileCenterCoordintes("rightup");
            this.StartCoroutine(this.AddTerrain(this._place));
        }
        public void OnClickMoveRightDown()
        {
            this._place = GetLocalTileCenterCoordintes("rightdown");
            this.StartCoroutine(this.AddTerrain(this._place));
        }

        /*
        public void OnClickCall()
        {
             GameObject terrain = GameObject.Find("terrain");
             StableMenu buttonObject = Instantiate(buttonCallPrefab) as StableMenu;
             buttonObject.transform.parent = terrain.transform;
        }
        */

        public void OnChangeDimension(string dimension)
        {
            currentDimension = dimension;
            this.StartCoroutine(this.AddTerrain(this._place));
        }

        public void OnChangeMapstyle(string mapstyle)
        {
            currentStyle = mapstyle;
            this.StartCoroutine(this.AddTerrain(this._place));
        }
        


        public void OnClickShowAddress(Vector3 position)
        {
            this.StartCoroutine(this.AddStreetAddress(position - new Vector3(0f, 0.2f, 0f)));
        }
		
		private IEnumerator CheckExistmap(string url)
        {
            url += "?action=maploaded";
            UnityWebRequest hs_get = UnityWebRequest.Get(url);
            yield return hs_get.SendWebRequest();
            if (hs_get.error != "" && hs_get.error != null)
            {
                Debug.Log(hs_get.error);
            }
            else
            {
                string xml = hs_get.downloadHandler.text;
                if (xml != "" && xml != mapName)
                {
                    _NeedReloadMap = true;
                    reloadmapname = xml;
                }
            }
        }

        private IEnumerator Updatemap(string url)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("mapName", mapName));
            formData.Add(new MultipartFormDataSection("loadBy", SystemInfo.deviceName));
            UnityWebRequest www = UnityWebRequest.Post(url, formData);
            www.chunkedTransfer = false;
            yield return www.SendWebRequest();
            if (www.error != "" && www.error != null)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form update complete!" + www.downloadHandler.text);
            }
        }

        /*
        private IEnumerator Eraseitems(string url)
        {
            url += "?action=deleteall";
            UnityWebRequest hs_get = UnityWebRequest.Get(url);
            yield return hs_get.SendWebRequest();
            if (hs_get.error != "" && hs_get.error != null)
            {
                Debug.Log(hs_get.error);
            }
            else
            {
                Debug.Log("Form delete all items complete!");
            }
        }
        */

        private Place GetLocalTileCenterCoordintes(string direction)
        {
            var tileUL = this._place.Location.ToTile(this._place.Level);

            switch (direction)
            {
                case "left":
                    tileUL.X -= 1;
                    break;
                case "right":
                    tileUL.X += 1;
                    break;
                case "down":
                    tileUL.Y += 1;
                    break;
                case "up":
                    tileUL.Y -= 1;
                    break;
                case "leftup":
                    tileUL.X -= 1;
                    tileUL.Y -= 1;
                    break;
                case "leftdown":
                    tileUL.X -= 1;
                    tileUL.Y += 1;
                    break;
                case "rightup":
                    tileUL.X += 1;
                    tileUL.Y -= 1;
                    break;
                case "rightdown":
                    tileUL.X += 1;
                    tileUL.Y += 1;
                    break;
                default:
                    return this._place;
            }

            var tileLR = new Tile()
            {
                Zoom = tileUL.Zoom,
                X = tileUL.X + CHILDREN_LEVEL * 2,
                Y = tileUL.Y + CHILDREN_LEVEL * 2
            };
            var coordUL = tileUL.UpperLeft(CHILDREN_LEVEL);
            var coordLR = tileLR.UpperLeft(CHILDREN_LEVEL);

            // Get tapped location relative to lower left.
            var coordCN = new Coordinate()
            {
                Latitude = coordUL.Latitude + (coordLR.Latitude - coordUL.Latitude) / 4f * 2.5f,
                Longitude = coordUL.Longitude + (coordLR.Longitude - coordUL.Longitude) / 4f * 2.5f,
            };

            this._place.Location = coordCN;
            return this._place;

        }
    }

}
