using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace HIO.Core
{
    /// <summary>
    /// A map that exposes commands in a WPF binding friendly manner
    /// </summary>
    [TypeDescriptionProvider(typeof(TCommandMapDescriptionProvider))]
    [System.Diagnostics.DebuggerStepThrough]
    public class TCommandMap
    {
        /// <summary>
        /// Add a named command to the command map
        /// </summary>
        /// <param name="commandName">The name of the command</param>
        /// <param name="executeMethod">The method to execute</param>
        /// <param name="canExecuteMethod">The method to execute to check if the command can be executed</param>
        public void AddCommand(string commandName, Action<object> executeMethod, Func<object, bool> canExecuteMethod = null)
        {
            Commands[commandName] = new TDelegateCommand(executeMethod, canExecuteMethod);
        }

        public void AddCommand(string commandName, Action executeMethod, Func<bool> canExecuteMethod = null)
        {
            Commands[commandName] = new TDelegateCommand(executeMethod, canExecuteMethod);
        }




        /// <summary>
        /// Remove a command from the command map
        /// </summary>
        /// <param name="commandName">The name of the command</param>
        public void RemoveCommand(string commandName)
        {
            Commands.Remove(commandName);
        }
        public void Update()
        {
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (_commands == null) return;
                foreach (var item in _commands.Values)
                {
                    item.Update();
                }
            }));
        }

        internal void AddCommand(string v1, object v2)
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Expose the dictionary of commands
        /// </summary>
        protected Dictionary<string, TDelegateCommand> Commands
        {
            get
            {
                if (null == _commands)
                    _commands = new Dictionary<string, TDelegateCommand>();

                return _commands;
            }
        }

        /// <summary>
        /// Store the commands
        /// </summary>
        private Dictionary<string, TDelegateCommand> _commands;

        /// <summary>
        /// Implements ICommand in a delegate friendly way
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public class TDelegateCommand : ICommand
        {

            /// <summary>
            /// Create a delegate command which executes the canExecuteMethod before executing the executeMethod
            /// </summary>
            /// <param name="executeMethod"></param>
            /// <param name="canExecuteMethod"></param>
            public TDelegateCommand(Action<object> executeMethod, Func<object, bool> canExecuteMethod = null)
            {
                _executeMethod = executeMethod;
                if (_executeMethod == null) throw new ArgumentNullException("executeMethod");
                _canExecuteMethod = canExecuteMethod;
            }
            public TDelegateCommand(Action executeMethod, Func<bool> canExecuteMethod = null)
            {
                if (executeMethod == null) throw new ArgumentNullException("executeMethod");
                _executeMethod = (x) => executeMethod();
                if (canExecuteMethod != null)
                {
                    _canExecuteMethod = (x) => canExecuteMethod();
                }
            }

            public bool CanExecute(object parameter)
            {
                return (null == _canExecuteMethod) ? true : _canExecuteMethod(parameter);
            }
            public void Update()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            public event EventHandler CanExecuteChanged;


            public void Execute(object parameter)
            {
                _executeMethod(parameter);
            }

            private Func<object, bool> _canExecuteMethod;
            private Action<object> _executeMethod;

        }


        /// <summary>
        /// Expose the dictionary entries of a CommandMap as properties
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        private class TCommandMapDescriptionProvider : TypeDescriptionProvider
        {
            /// <summary>
            /// Standard constructor
            /// </summary>
            public TCommandMapDescriptionProvider()
                : this(TypeDescriptor.GetProvider(typeof(TCommandMap)))
            {
            }

            /// <summary>
            /// Construct the provider based on a parent provider
            /// </summary>
            /// <param name="parent"></param>
            public TCommandMapDescriptionProvider(TypeDescriptionProvider parent)
                : base(parent)
            {
            }

            /// <summary>
            /// Get the type descriptor for a given object instance
            /// </summary>
            /// <param name="objectType">The type of object for which a type descriptor is requested</param>
            /// <param name="instance">The instance of the object</param>
            /// <returns>A custom type descriptor</returns>
            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            {
                return new TCommandMapDescriptor(base.GetTypeDescriptor(objectType, instance), instance as TCommandMap);
            }
        }

        /// <summary>
        /// This class is responsible for providing custom properties to WPF - in this instance
        /// allowing you to bind to commands by name
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        private class TCommandMapDescriptor : CustomTypeDescriptor
        {
            /// <summary>
            /// Store the command map for later
            /// </summary>
            /// <param name="descriptor"></param>
            /// <param name="map"></param>
            public TCommandMapDescriptor(ICustomTypeDescriptor descriptor, TCommandMap map)
                : base(descriptor)
            {
                _map = map;
            }

            /// <summary>
            /// Get the properties for this command map
            /// </summary>
            /// <returns>A collection of synthesized property descriptors</returns>
            public override PropertyDescriptorCollection GetProperties()
            {
                //TODO: See about caching these properties (need the _map to be observable so can respond to add/remove)
                PropertyDescriptor[] props = new PropertyDescriptor[_map.Commands.Count];

                int pos = 0;

                foreach (KeyValuePair<string, TDelegateCommand> command in _map.Commands)
                    props[pos++] = new TCommandPropertyDescriptor(command);

                return new PropertyDescriptorCollection(props);
            }

            private TCommandMap _map;
        }

        /// <summary>
        /// A property descriptor which exposes an ICommand instance
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        private class TCommandPropertyDescriptor : PropertyDescriptor
        {
            /// <summary>
            /// Construct the descriptor
            /// </summary>
            /// <param name="command"></param>
            public TCommandPropertyDescriptor(KeyValuePair<string, TDelegateCommand> command)
                : base(command.Key, null)
            {
                _command = command.Value;
            }

            /// <summary>
            /// Always read only in this case
            /// </summary>
            public override bool IsReadOnly
            {
                get { return true; }
            }

            /// <summary>
            /// Nope, it's read only
            /// </summary>
            /// <param name="component"></param>
            /// <returns></returns>
            public override bool CanResetValue(object component)
            {
                return false;
            }

            /// <summary>
            /// Not needed
            /// </summary>
            public override Type ComponentType
            {
                get { throw new NotImplementedException(); }
            }

            /// <summary>
            /// Get the ICommand from the parent command map
            /// </summary>
            /// <param name="component"></param>
            /// <returns></returns>
            public override object GetValue(object component)
            {
                TCommandMap map = component as TCommandMap;

                if (null == map)
                    throw new ArgumentException("component is not a CommandMap instance", "component");

                return map.Commands[Name];
            }

            /// <summary>
            /// Get the type of the property
            /// </summary>
            public override Type PropertyType
            {
                get { return typeof(TDelegateCommand); }
            }

            /// <summary>
            /// Not needed
            /// </summary>
            /// <param name="component"></param>
            public override void ResetValue(object component)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Not needed
            /// </summary>
            /// <param name="component"></param>
            /// <param name="value"></param>
            public override void SetValue(object component, object value)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Not needed
            /// </summary>
            /// <param name="component"></param>
            /// <returns></returns>
            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            /// <summary>
            /// Store the command which will be executed
            /// </summary>
            private TDelegateCommand _command;
        }
    }
}
