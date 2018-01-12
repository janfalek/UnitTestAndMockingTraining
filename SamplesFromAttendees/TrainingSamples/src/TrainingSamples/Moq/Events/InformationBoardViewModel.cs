using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using AutoMapper;
using Caliburn.Micro;
using Action = System.Action;

namespace TrainingSamples.Moq.Events
{
    public class InformationBoardViewModel : PropertyChangedBase, IInformationBoardViewModel, IHandle<InformationBoardMessageBase>
    {
        private readonly IMapper mapper;
        private readonly IEventAggregator eventAggregator;
        private readonly List<InformatioBoardItemViewModelBase> allInformationBoardMessages
            = new List<InformatioBoardItemViewModelBase>();

        private InformationBoardMessageType informationBoardMessageTypeFilter = InformationBoardMessageType.None;

        public InformationBoardViewModel(IMapper mapper, IEventAggregator eventAggregator)
        {
            this.mapper = mapper;
            this.eventAggregator = eventAggregator;
            InformationBoardMessageTypeFilter = InformationBoardMessageType.Information;
        }

        public event System.Action ContentCollapsedAction;
        public event System.Action ContentExpandedAction;

        public string MessagesCounterText =>
            $"Messages {allInformationBoardMessages.Count(ibm => ibm.InformationBoardMessageType == InformationBoardMessageType.Information)}";

        public string ErrorsCounterText =>
            $"Errors {allInformationBoardMessages.Count(ibm => ibm.InformationBoardMessageType == InformationBoardMessageType.Error)}";

        public string WarningsCounterText =>
            $"Warnings {allInformationBoardMessages.Count(ibm => ibm.InformationBoardMessageType == InformationBoardMessageType.Warning)}";

        public string InformationBoardExpanderName => "InformationBoardExpander";

        public ObservableCollection<InformatioBoardItemViewModelBase> InformationBoardMessages { get; set; } = new ObservableCollection<InformatioBoardItemViewModelBase>();

        public InformationBoardMessageType InformationBoardMessageTypeFilter
        {
            get
            {
                return informationBoardMessageTypeFilter;
            }
            set
            {
                informationBoardMessageTypeFilter = value;
                NotifyOfPropertyChange(() => InformationBoardMessageTypeFilter);
                RefreshMessageBoardList();
            }
        }

        public void Initialize()
        {
            eventAggregator.Subscribe(this);
        }

        public void ContentCollapsed(RoutedEventArgs eventArgs)
        {
            if (((FrameworkElement)eventArgs.OriginalSource).Name == InformationBoardExpanderName)
            {
                ContentCollapsedAction?.Invoke();
            }
        }

        public void ContentExpanded(RoutedEventArgs eventArgs)
        {
            if (((FrameworkElement)eventArgs.OriginalSource).Name == InformationBoardExpanderName)
            {
                ContentExpandedAction?.Invoke();
            }
        }

        public void Handle(InformationBoardMessageBase message)
        {
            var informationBoardItemViewModel = allInformationBoardMessages.FirstOrDefault(ibm => ibm.MessageId == message.MessageId);

            if (informationBoardItemViewModel != null)
            {
                mapper.Map(message, informationBoardItemViewModel);
            }
            else
            {
                informationBoardItemViewModel = mapper
                    .Map<InformationBoardMessageBase, InformatioBoardItemViewModelBase>(message);

                allInformationBoardMessages.Add(informationBoardItemViewModel);
            }

            informationBoardItemViewModel.OnClose += () =>
            {
                RemoveInformationBoardItem(informationBoardItemViewModel);
            };
            RefreshMessageBoardList();
            RefreshCounters();
        }

        public void RemoveInformationBoardItem(InformatioBoardItemViewModelBase informationBoardItemViewModel)
        {
            allInformationBoardMessages.Remove(informationBoardItemViewModel);
            RefreshMessageBoardList();
            RefreshCounters();
        }

        private void RefreshMessageBoardList()
        {
            InformationBoardMessages.Clear();
            foreach (var informationBoardMessage in
                allInformationBoardMessages
                .Where(ibm => (ibm.InformationBoardMessageType & informationBoardMessageTypeFilter) != 0)
                .OrderByDescending(ibm => ibm.MessageDate))
            {
                InformationBoardMessages.Add(informationBoardMessage);
            }
        }

        private void RefreshCounters()
        {
            NotifyOfPropertyChange(() => MessagesCounterText);
            NotifyOfPropertyChange(() => ErrorsCounterText);
            NotifyOfPropertyChange(() => WarningsCounterText);
        }
    }

    public interface IInformationBoardViewModel
    {
        event Action ContentCollapsedAction;
        event Action ContentExpandedAction;

        void Initialize();
    }

    public abstract class InformationBoardMessageBase
    {
        private Guid? messageId;

        public Guid MessageId
        {
            get
            {
                if (!messageId.HasValue)
                    messageId = Guid.NewGuid();

                return messageId.Value;
            }
            set
            {
                messageId = value;
            }
        }

        public string Header { get; set; }
        public string Content { get; set; }
        public string CallBackActionName { get; set; }
        public string CallingPlugin { get; set; }
        public DateTime MessageDate { get; } = DateTime.Now;
    }

    public abstract class InformatioBoardItemViewModelBase
    {
        private readonly IPluginActionSender plguinActionSender;

        protected InformatioBoardItemViewModelBase(IPluginActionSender plguinActionSender)
        {
            this.plguinActionSender = plguinActionSender;
        }

        public event Action OnClose;

        public Guid MessageId { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }
        public abstract InformationBoardMessageType InformationBoardMessageType { get; }
        public string CallBackActionName { get; set; }
        public string CallingPlugin { get; set; }
        public DateTime MessageDate { get; set; }

        public string MessageDateString => MessageDate.ToString("H:mm:ss dd.MM.yyyy");

        public bool HasNavigationLink =>
            !string.IsNullOrWhiteSpace(CallBackActionName) && !string.IsNullOrWhiteSpace(CallingPlugin);

        public void NavigateToPlugin()
        {
            var message = new PluginActionMessage
            {
                ActionId = CallBackActionName,
                PluginName = CallingPlugin
            };

            plguinActionSender.SendActionToPlugin(message);
        }

        public void Close()
        {
            OnClose?.Invoke();
        }
    }

    public interface IPluginActionSender
    {
        void SendActionToPlugin(PluginActionMessage message);

        void SendPopupResult(DialogResult dialogResult);

        void TerminatePlugins();

        void CancelBackgroundWorker();
    }

    public enum DialogResult
    {
        /// <summary>
        /// Ok
        /// </summary>
        Ok = 10,

        /// <summary>
        /// Yes
        /// </summary>
        Yes = 20,

        /// <summary>
        /// No
        /// </summary>
        No = 30,

        /// <summary>
        /// Cancel
        /// </summary>
        Cancel = 40
    }

    [DataContract]
    public class PluginActionMessage
    {
        [DataMember]
        public string PluginName { get; set; }

        [DataMember]
        public string ActionId { get; set; }
    }

    [Flags]
    public enum InformationBoardMessageType
    {
        None = 0,
        Information = 1,
        Error = 2,
        Warning = 4,
        All = 7
    }
}