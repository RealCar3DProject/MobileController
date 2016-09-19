using UnityEngine;
using System.Collections.Generic;
using Byn.Common;

namespace Byn.Net
{
    
    //TODO: List that keeps track of created networks will be moved into the
    //underlaying factories
    public class WebRtcNetworkFactory : UnitySingleton<WebRtcNetworkFactory>
    {

        //android needs a static init process. 
        /// <summary>
        /// True if the platform specific init process was tried
        /// </summary>
        private static bool sStaticInitTried = false;

        /// <summary>
        /// true if the static init process was successful. false if not yet tried or failed.
        /// </summary>
        private static bool sStaticInitSuccessful = false;

        private IWebRtcNetworkFactory mFactory = null;
        private List<IBasicNetwork> mCreatedNetworks = new List<IBasicNetwork>();

        public static bool StaticInitSuccessful
        {
            get
            {
                return sStaticInitSuccessful;
            }
        }

        private WebRtcNetworkFactory()
        {

            //try to setup (this also checks if the platform is even supported)
            TryStaticInitialize();
            //setup failed? factory will be null so nothing can be created
            if (sStaticInitSuccessful == false)
            {
                Debug.LogError("Initialization of the webrtc plugin failed. StaticInitSuccessful is false. ");
                mFactory = null;
                return;
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            
            mFactory = new Byn.Net.BrowserWebRtcNetworkFactory();
#else
            LogNativeSupportInfo();
            Byn.Net.Native.NativeWebRtcNetworkFactory factory = new Byn.Net.Native.NativeWebRtcNetworkFactory();
            factory.Initialize(true);
            mFactory = factory;
            Debug.Log("Using Wrapper: " + WebRtcCSharp.WebRtcWrap.GetVersion() + " WebRTC: " + WebRtcCSharp.WebRtcWrap.GetWebRtcVersion());
            
#endif

        }

        /// <summary>
        /// Internal use only!
        /// Used to check if the current platform is supported
        /// Used for other libraries building on top of WebRtcNetwork.
        /// </summary>
        /// <returns>True if the platform is supported, false if not.</returns>
        public static bool CheckNativeSupport()
        {
            //do not access any platform specific code here. only check if there if the platform is supported
            //keep up to date with LogNativeSupportInfo()
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return true;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            return true;
#elif UNITY_ANDROID
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Prints platform specific info to the unity log
        /// </summary>
        internal static void LogNativeSupportInfo()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            Debug.Log("Initializing native webrtc ...");
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            Debug.LogWarning("Trying to initialize native webrtc. Note that support for your OSX is not yet stable!");
#elif UNITY_ANDROID
            Debug.LogWarning("Trying to initialize native webrtc. Note that support for your android is not yet stable!");
#else
            Debug.LogError("Platform not supported!");
#endif
        }

        /// <summary>
        /// Internally used. No need to manually call this.
        /// 
        /// This function will initialize the wrapper (if needed) before the first webrtc factory can be created.
        /// 
        /// It will set sStaticInitSuccessful to false if the platform isn't supported or the init process failed.
        /// 
        /// </summary>
        public static void TryStaticInitialize()
        {
            //make sure it is called only once. no need for multiple static inits...
            if (sStaticInitTried)
                return;
            sStaticInitTried = true;


            
#if UNITY_WEBGL && !UNITY_EDITOR

            //check if the java script part is available
            if (BrowserWebRtcNetwork.IsAvailable() == false)
            {
                //js part is missing -> inject the code into the browser
                BrowserWebRtcNetwork.InjectJsCode();
            }
            //if still not available something failed. setting sStaticInitSuccessful to false
            //will block the use of the factories
            sStaticInitSuccessful = BrowserWebRtcNetwork.IsAvailable();
            if(sStaticInitSuccessful == false)
            {
                Debug.LogError("Failed to create the call factory. This might be because of browser incompatibility or a missing java script plugin!");
                return;
            }
#else

            LogNativeSupportInfo();
            Debug.Log("Using Wrapper: " + WebRtcCSharp.WebRtcWrap.GetVersion() + " WebRTC: " + WebRtcCSharp.WebRtcWrap.GetWebRtcVersion());
            sStaticInitSuccessful = CheckNativeSupport();
            if (sStaticInitSuccessful == false)
                return;
#endif

            //android version needs a special init method in addition
#if UNITY_ANDROID && !UNITY_EDITOR
            sStaticInitSuccessful = TryInitAndroid();
            if (sStaticInitSuccessful == false)
                return;
#endif
        }

        private static bool TryInitAndroid()
        {
            

            Debug.Log("TryInitAndroid");


            Debug.Log("get activity");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

            Debug.Log("call InitAndroidContext");
            bool successful = WebRtcCSharp.RTCPeerConnectionFactory.InitAndroidContext(context.GetRawObject());

            if (successful)
            {
                Debug.Log("Android plugin successful initialized.");
            }
            else
            {
                Debug.LogError("Failed to initialize android plugin.");
            }

            return successful;
        }

        public IBasicNetwork CreateDefault(string websocketUrl, string[] urls = null)
        {
            IBasicNetwork network = mFactory.CreateDefault(websocketUrl, urls);
            mCreatedNetworks.Add(network);
            return network;
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();

            Debug.Log("Network factory is being destroyed. All created basic networks will be destroyed as well!");
            foreach (IBasicNetwork net in mCreatedNetworks)
            {
                net.Dispose();
            }
            mCreatedNetworks.Clear();

            //cleanup
            if (mFactory != null)
            {
                mFactory.Dispose();
            }
        }
    }
}