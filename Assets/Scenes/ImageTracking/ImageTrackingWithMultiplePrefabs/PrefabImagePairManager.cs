using System;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using System.Collections;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
    /// and overlays some prefabs on top of the detected image.
    /// </summary>
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class PrefabImagePairManager : MonoBehaviour, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Used to associate an `XRReferenceImage` with a Prefab by using the `XRReferenceImage`'s guid as a unique identifier for a particular reference image.
        /// </summary>
        [Serializable]
        struct NamedPrefab
        {
            // System.Guid isn't serializable, so we store the Guid as a string. At runtime, this is converted back to a System.Guid
            public string imageGuid;
            public GameObject imagePrefab;

            public NamedPrefab(Guid guid, GameObject prefab)
            {
                imageGuid = guid.ToString();
                imagePrefab = prefab;
            }
        }

        [SerializeField] 
        [HideInInspector]
        List<NamedPrefab> m_PrefabsList = new List<NamedPrefab>();

        Dictionary<Guid, GameObject> m_PrefabsDictionary = new Dictionary<Guid, GameObject>();
        Dictionary<Guid, GameObject> m_Instantiated = new Dictionary<Guid, GameObject>();
        ARTrackedImageManager m_TrackedImageManager;

        [SerializeField]
        [Tooltip("Reference Image Library")]
        XRReferenceImageLibrary m_ImageLibrary;

        //[SerializeField]public TextMeshPro textMeshPro;
        private float activeDuration = 3.0f;
        public GameObject checkScanGO;

        /// <summary>
        /// Get the <c>XRReferenceImageLibrary</c>
        /// </summary>
        public XRReferenceImageLibrary imageLibrary
        {
            get => m_ImageLibrary;
            set => m_ImageLibrary = value;
        }

        public void OnBeforeSerialize()
        {
            m_PrefabsList.Clear();
            foreach (var kvp in m_PrefabsDictionary)
            {
                m_PrefabsList.Add(new NamedPrefab(kvp.Key, kvp.Value));
            }
        }

        public void OnAfterDeserialize()
        {
            m_PrefabsDictionary = new Dictionary<Guid, GameObject>();
            foreach (var entry in m_PrefabsList)
            {
                m_PrefabsDictionary.Add(Guid.Parse(entry.imageGuid), entry.imagePrefab);
            }
        }
        void Start()
        {
            checkScanGO = GameObject.Find("CheckText");
            if (checkScanGO == null) checkScanGO = GameObject.FindGameObjectWithTag("checktext");
            //TextMeshProUGUI CheckScanText = checkScanGO.GetComponent<TextMeshProUGUI>();
            checkScanGO.SetActive(false);
        }
        void Awake()
        {
            m_TrackedImageManager = GetComponent<ARTrackedImageManager>();

        }

        void OnEnable()
        {
            m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }

        void OnDisable()
        {
            m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }

        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var trackedImage in eventArgs.added)
            {
                // Give the initial image a reasonable default scale
                var minLocalScalar = Mathf.Min(trackedImage.size.x, trackedImage.size.y) / 2;
                trackedImage.transform.localScale = new Vector3(minLocalScalar, minLocalScalar, minLocalScalar);
                AssignPrefab(trackedImage);
            }
        }

        IEnumerator ActivateTextForDuration()
        {
            //GameObject checkScanGO = GameObject.Find("CheckText");
            //if(checkScanGO == null) checkScanGO = GameObject.FindGameObjectWithTag("checktext");
            //TextMeshProUGUI CheckScanText = checkScanGO.GetComponent<TextMeshProUGUI>();
            checkScanGO.SetActive(true);

            // Activate the TextMeshPro
            //textMeshPro.gameObject.SetActive(true);

            // Wait for the specified duration
            yield return new WaitForSeconds(activeDuration);

            // Deactivate the TextMeshPro after the duration
            checkScanGO.SetActive(false);
            //textMeshPro.gameObject.SetActive(false);
        }
        /// <summary>
        /// MY NOTE: CHANGE THIS METHOD TO DEFINE POP UP PREFAB
        /// </summary>
        /// <param name="trackedImage"></param>
        void AssignPrefab(ARTrackedImage trackedImage)
        {
            
            if (m_PrefabsDictionary.TryGetValue(trackedImage.referenceImage.guid, out var prefab))
            {
                //1809: TO DO: place the prefab at the correct location
                //INSTANTIATE THE WHOLE ZURICH MODEL WITH 50 INDIVIDUAL HOUSES ALTOGETHER
                Vector3 model_loc = trackedImage.transform.position + new Vector3(-0.2f, 0, 0.6f);//+ new Vector3(-0.5f, 1f, 0f);flyingabove///new Vector3(-5f, 0, 10f);TooFar(left up above the table)//new Vector3(-0.5f, 0f, 1f);//(-2f, 2f, 0f); //(0.01f,1f,0.01f);
                               

                Debug.Log("I enabled the prefab instantiating");
                m_Instantiated[trackedImage.referenceImage.guid] = Instantiate(prefab, model_loc, trackedImage.transform.rotation * prefab.transform.rotation);// *Quaternion.identity);
                                                                                                                                                               //transform.rotation * Quaternion.Euler(270f, 180f, 0f));//(0f, 180f, 180f));//Quaternion.identity);

                //m_Instantiated[trackedImage.referenceImage.guid] = Instantiate(prefab, trackedImage.transform);
                GameObject.Find("ScanIndicatorPanel").SetActive(false);

                StartCoroutine(ActivateTextForDuration());
                //GameObject checkScanGO = GameObject.FindGameObjectWithTag("checktext");
                //TextMeshProUGUI CheckScanText = checkScanGO.GetComponent<TextMeshProUGUI>();
                //CheckScanText.text = "Scan Succeed!";
              

            }
        }

        public GameObject GetPrefabForReferenceImage(XRReferenceImage referenceImage)
            => m_PrefabsDictionary.TryGetValue(referenceImage.guid, out var prefab) ? prefab : null;

        public void SetPrefabForReferenceImage(XRReferenceImage referenceImage, GameObject alternativePrefab)
        {
            m_PrefabsDictionary[referenceImage.guid] = alternativePrefab;
            if (m_Instantiated.TryGetValue(referenceImage.guid, out var instantiatedPrefab))
            {
                m_Instantiated[referenceImage.guid] = Instantiate(alternativePrefab, instantiatedPrefab.transform.parent);
                Destroy(instantiatedPrefab);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// This customizes the inspector component and updates the prefab list when
        /// the reference image library is changed.
        /// </summary>
        [CustomEditor(typeof(PrefabImagePairManager))]
        class PrefabImagePairManagerInspector : Editor
        {
            List<XRReferenceImage> m_ReferenceImages = new List<XRReferenceImage>();
            bool m_IsExpanded = true;

            bool HasLibraryChanged(XRReferenceImageLibrary library)
            {
                if (library == null)
                    return m_ReferenceImages.Count == 0;

                if (m_ReferenceImages.Count != library.count)
                    return true;

                for (int i = 0; i < library.count; i++)
                {
                    if (m_ReferenceImages[i] != library[i])
                        return true;
                }

                return false;
            }

            public override void OnInspectorGUI()
            {
                //customized inspector
                var behaviour = serializedObject.targetObject as PrefabImagePairManager;

                serializedObject.Update();
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
                }

                var libraryProperty = serializedObject.FindProperty(nameof(m_ImageLibrary));
                EditorGUILayout.PropertyField(libraryProperty);
                var library = libraryProperty.objectReferenceValue as XRReferenceImageLibrary;

                //check library changes
                if (HasLibraryChanged(library))
                {
                    if (library)
                    {
                        var tempDictionary = new Dictionary<Guid, GameObject>();
                        foreach (var referenceImage in library)
                        {
                            tempDictionary.Add(referenceImage.guid, behaviour.GetPrefabForReferenceImage(referenceImage));
                        }
                        behaviour.m_PrefabsDictionary = tempDictionary;
                    }
                }

                // update current
                m_ReferenceImages.Clear();
                if (library)
                {
                    foreach (var referenceImage in library)
                    {
                        m_ReferenceImages.Add(referenceImage);
                    }
                }

                //show prefab list
                m_IsExpanded = EditorGUILayout.Foldout(m_IsExpanded, "Prefab List");
                if (m_IsExpanded)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUI.BeginChangeCheck();

                        var tempDictionary = new Dictionary<Guid, GameObject>();
                        foreach (var image in library)
                        {
                            var prefab = (GameObject) EditorGUILayout.ObjectField(image.name, behaviour.m_PrefabsDictionary[image.guid], typeof(GameObject), false);
                            tempDictionary.Add(image.guid, prefab);
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(target, "Update Prefab");
                            behaviour.m_PrefabsDictionary = tempDictionary;
                            EditorUtility.SetDirty(target);
                        }
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
