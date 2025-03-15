using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

namespace Farm
{

    /// <summary>
    /// Handle the cycle of Day and Night. Everything that need to change across time will register itself to this handler
    /// which will update it when it update (e.g. ShadowInstance, Interpolator etc.).
    /// The ticking of that system can be stopped, this is useful e.g. if the game is put in pause (or need to do cutscene
    /// etc..)
    /// </summary>
    [DefaultExecutionOrder(10)]
    public class DayCycleHandler : MonoBehaviour
    {
        private Transform LightsRoot;

        private Light2D DayLight;
        public Gradient DayLightGradient;

        private Light2D NightLight;
        public Gradient NightLightGradient;

        private Light2D AmbientLight;
        public Gradient AmbientLightGradient;

        private Light2D SunRimLight;
        public Gradient SunRimLightGradient;

        private Light2D MoonRimLight;
        public Gradient MoonRimLightGradient;

        [Tooltip("The angle 0 = upward, going clockwise to 1 along the day")]
        public AnimationCurve ShadowAngle;
        [Tooltip("The scale of the normal shadow length (0 to 1) along the day")]
        public AnimationCurve ShadowLength;
        

        private void Awake()
        {
            GameManager.Instance.DayCycleHandler = this;
            LightsRoot = transform;
            DayLight = transform.Find("DayLight").GetComponent<Light2D>();
            NightLight = transform.Find("NightLight").GetComponent<Light2D>();
            AmbientLight = transform.Find("AmbientLight").GetComponent<Light2D>();
            SunRimLight = transform.Find("DayLightRim").GetComponent<Light2D>();
            MoonRimLight = transform.Find("NightLightRim").GetComponent<Light2D>();
        }

        /// <summary>
        /// We use an explicit ticking function instead of update so the GameManager can potentially freeze or change how
        /// time pass
        /// </summary>
        public void Tick()
        {
            UpdateLight(GameManager.Instance.CurrentDayRatio);
        }

        public void UpdateLight(float ratio)
        {
            DayLight.color = DayLightGradient.Evaluate(ratio);
            NightLight.color = NightLightGradient.Evaluate(ratio);
            AmbientLight.color = AmbientLightGradient.Evaluate(ratio);
            SunRimLight.color = SunRimLightGradient.Evaluate(ratio);
            MoonRimLight.color = MoonRimLightGradient.Evaluate(ratio);
            LightsRoot.rotation = Quaternion.Euler(0,0, 360.0f * ratio);
            UpdateShadow(ratio);
        }

        void UpdateShadow(float ratio)
        {
            var currentShadowAngle = ShadowAngle.Evaluate(ratio);
            var currentShadowLength = ShadowLength.Evaluate(ratio);

            var opposedAngle = currentShadowAngle + 0.5f;
            while (currentShadowAngle > 1.0f)
                currentShadowAngle -= 1.0f;
        }
    }
    
    
#if UNITY_EDITOR
    // Wrapping a custom editor between UNITY_EDITOR define check allow to keep it in the same 
    // file as this part will be stripped when building for standalone (where Editor class doesn't exist).
    // Don't forget to also wrap the UnityEditor using at the top of the file between those define check too.
    
    // Show a slider that allow to test a specific time to help define colors.
    [CustomEditor(typeof(DayCycleHandler))]
    class DayCycleEditor : Editor
    {
        private DayCycleHandler m_Target;

        public override VisualElement CreateInspectorGUI()
        {
            m_Target = target as DayCycleHandler;

            var root = new VisualElement();
            
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            
            var slider = new Slider(0.0f, 1.0f);
            slider.label = "Test time 0:00";
            slider.RegisterValueChangedCallback((EventCallback<ChangeEvent<float>>)(evt =>
            {
                m_Target.UpdateLight(evt.newValue);
                
                //slider.label = $"Test Time {DayEvent.GetTimeAsString(evt.newValue)} ({evt.newValue:F2})";
                SceneView.RepaintAll();
            }));
            
            //registering click event, it's very catch all but not way to do a change check for control change
            root.RegisterCallback<ClickEvent>(evt =>
            {
                Debug.LogError(slider.value);
                m_Target.UpdateLight(slider.value);
                SceneView.RepaintAll();
            });
            
            root.Add(slider);

            return root;
        }
    }
#endif

}