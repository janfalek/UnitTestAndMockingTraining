//Test InitializeInformationBoard
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Interop;
using Caliburn.Micro;
using TrainingSamples.BackgroundWorker;
using TrainingSamples.Moq.Setup;

namespace TrainingSamples.Moq.Events
{
    public class MainWindowViewModel :
        Conductor<object>,
        IMainViewCommunicator,
        IHandle<PluginContainerInitialized>,
        IHandle<HostClose>,
        IHandle<DesignContextChanged>,
        IHandleWithTask<PluginTermination>,
        IHandle<SetWindowFocus>,
        IHandle<ProgressMessage>
    {
        public const double DefaultInformationBoardRowHeight = 200;
        public const double MinInformationBoardHeight = 48;

        private readonly IHostCommunicationServer hostCommunicationServer;
        private readonly IPluginActionSender pluginActionSender;
        private readonly IHostNotificationService hostNotificationService;
        private readonly IAppDomainCreator appDomainCreator;
        private readonly IEventAggregator eventAggregator;
        private readonly IHostConfigurationProvider hostConfigurationProvider;
        private readonly IPluginResponseTimeout pluginResponseTimeoutService;
        private readonly IAppSetting appSetting;
        private readonly IWindowManager windowManager;
        private string endpoint;
        private bool biVisible;
        private bool popupVisible = false;
        private bool isGridSplitterEnabled = true;

        private double currentInformationBoardRowHeight = DefaultInformationBoardRowHeight;
        private double previousInformationBoardRowHeight;

        public MainWindowViewModel(
            IHostCommunicationServer hostCommunicationServer,
            IPluginActionSender pluginActionSender,
            IHostNotificationService hostNotificationService,
            IAppDomainCreator appDomainCreator,
            IEventAggregator eventAggregator,
            IHostConfigurationProvider hostConfigurationProvider,
            IPluginResponseTimeout pluginResponseTimeoutService,
            INavigationMenuViewModel navigationMenuViewModel,
            IToolboxMenuViewModel toolboxMenuViewModel,
            IInformationBoardViewModel informationBoardViewModel,
            IAppSetting appSetting,
            IChildWindowManager childWindowManager,
            IWindowFocusSetter windowFocusSetter,
            IWindowManager windowManager)
        {
            this.hostCommunicationServer = hostCommunicationServer;
            this.pluginActionSender = pluginActionSender;
            this.hostNotificationService = hostNotificationService;
            this.appDomainCreator = appDomainCreator;
            this.eventAggregator = eventAggregator;
            this.hostConfigurationProvider = hostConfigurationProvider;
            this.pluginResponseTimeoutService = pluginResponseTimeoutService;
            this.appSetting = appSetting;
            ChildWindowManager = childWindowManager;
            WindowFocusSetter = windowFocusSetter;
            this.windowManager = windowManager;

            NavigationMenuViewModel = navigationMenuViewModel;
            ToolboxMenuViewModel = toolboxMenuViewModel;
            InformationBoardViewModel = informationBoardViewModel;
        }

        private double top1;
        private double left1;

        public Window Window => (Window)GetView();
        public IWindowWorkSpace WindowWorkSpace => (IWindowWorkSpace)GetView();

        public double Top1
        {
            get { return top1; }
            set
            {
                if (value == top1) return;
                top1 = value;
                NotifyOfPropertyChange(() => Top1);
            }
        }

        public double Left1
        {
            get { return left1; }
            set
            {
                if (value.Equals(left1)) return;
                left1 = value;
                NotifyOfPropertyChange(() => Left1);
            }
        }

        public void MovePopup()
        {
            if (popupVisible)
            {
           //     BusyIndicatorWindowViewModel.MoveWindow(WindowWorkSpace.GetCoordinates());
            }

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        // Make sure RECT is actually OUR defined struct, not the windows rect.
        public Point GetWindowpoint()
        {
            RECT rect;
            GetWindowRect((new WindowInteropHelper(Window)).Handle, out rect);

            return new Point(rect.Left, rect.Top);
        }

        public event Action<IntPtr?> WindowGrab;

        public event System.Action WindowClose;

        public event System.Action WindowFocusSet;

        public double MinHeightProperty { get; } = MinInformationBoardHeight;

        public bool IsGridSplitterEnabled
        {
            get
            {
                return isGridSplitterEnabled;
            }
            set
            {
                isGridSplitterEnabled = value;
                NotifyOfPropertyChange(() => IsGridSplitterEnabled);
            }
        }

        public double CurrentInformationBoardRowHeight
        {
            get
            {
                return currentInformationBoardRowHeight;
            }
            set
            {
                currentInformationBoardRowHeight = value;
                NotifyOfPropertyChange(() => CurrentInformationBoardRowHeight);
            }
        }

        public INavigationMenuViewModel NavigationMenuViewModel { get; }

        public IToolboxMenuViewModel ToolboxMenuViewModel { get; }

        public IInformationBoardViewModel InformationBoardViewModel { get; }

        public IChildWindowManager ChildWindowManager { get; }

        public IWindowFocusSetter WindowFocusSetter { get; }

      //  public BusyIndicatorWindowViewModel BusyIndicatorWindowViewModel { get; set; }
       // public BusyIndicatorViewModel BusyIndicatorViewModel { get; set; }

        public string Version => "Version xxx";

        public string Footer { get; set; } = "Information Board";

        public void Handle(PluginContainerInitialized message)
        {
            OnWindowGrab(message.PluginWindowPointer);
        }

        public void Handle(DesignContextChanged message)
        {
            var timeouteSetting = appSetting.GetSetting("MainWindow.PluginResponseTimeout", 15);
            pluginResponseTimeoutService.SetResponseTimeout(timeouteSetting);
            OnWindowGrab(null);
            NavigationMenuViewModel.ResetMenuState();
            ToolboxMenuViewModel.ResetMenuState();
            pluginActionSender.TerminatePlugins();
        }

        public void Handle(HostClose message)
        {
            OnWindowClose();
        }

        public async Task Handle(PluginTermination message)
        {
            pluginResponseTimeoutService.CancelTimeout();
            hostNotificationService.ReleaseCallbacks();

            await Task.Run(() =>
            {
                appDomainCreator.UnloadPlugin();
                CreatePluginContainer();
            });
        }

        public void Handle(SetWindowFocus message)
        {
            OnWindowFocusSet();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            eventAggregator.Subscribe(this);
            InitializeInformationBoard();

//            eventAggregator.Subscribe(BusyIndicatorViewModel);
//            eventAggregator.Subscribe(BusyIndicatorWindowViewModel);
            BIVisible = false;
            NavigationMenuViewModel.Initialize();
            ToolboxMenuViewModel.Initialize();
            StartCommunicationServer();
            CreatePluginContainer();
        }

        protected virtual void OnWindowGrab(IntPtr? obj)
        {
            WindowGrab?.Invoke(obj);
        }

        protected virtual void OnWindowClose()
        {
            WindowClose?.Invoke();
        }

        protected virtual void OnWindowFocusSet()
        {
            WindowFocusSet?.Invoke();
        }

        private void InitializeInformationBoard()
        {
            InformationBoardViewModel.Initialize();

            InformationBoardViewModel.ContentCollapsedAction += () =>
            {
                IsGridSplitterEnabled = false;
                previousInformationBoardRowHeight = CurrentInformationBoardRowHeight;
                CurrentInformationBoardRowHeight = MinInformationBoardHeight;
            };

            InformationBoardViewModel.ContentExpandedAction += () =>
            {
                IsGridSplitterEnabled = true;
                CurrentInformationBoardRowHeight = previousInformationBoardRowHeight;
            };
        }

        private void CreatePluginContainer()
        {
            appDomainCreator.CreateSubDomain(endpoint, hostConfigurationProvider.GetConfiguration());
        }

        private void StartCommunicationServer()
        {
            endpoint = hostCommunicationServer.Initialize();
        }

        public void Handle(ProgressMessage message)
        {
//            if (message.MessageType == BackgroundMessageType.InProgress && !popupVisible)
//            {
//                try
//                {
//                    windowManager.ShowWindow(BusyIndicatorWindowViewModel);
//                    BusyIndicatorWindowViewModel.Window.Owner = (VisualStyleElement.Window)GetView();
//                    popupVisible = true;
//                    MovePopup();
//                }
//                catch (Exception e)
//                {
//                    Console.WriteLine(e);
//                    throw;
//                }
//            }
//
//            if (message.MessageType == BackgroundMessageType.Finished && popupVisible)
//            {
//                popupVisible = false;
//                BusyIndicatorWindowViewModel.ClosePopup();
//            }
        }

        public bool BIVisible
        {
            get { return biVisible; }
            set
            {
                biVisible = value;
                NotifyOfPropertyChange("BIVisible");
            }
        }
    }
}