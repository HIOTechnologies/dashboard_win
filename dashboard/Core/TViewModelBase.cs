using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Threading;

namespace HIO.Core
{
    public class TViewModelBase : INotifyPropertyChanged
    {
        public TViewModelBase()
        {
            //TaskScheduler.
        }
        #region Fields

        private Dictionary<string, object> _PropertyValues = new Dictionary<string, object>();
        private TCommandMap _Commands;
         
        #endregion

        #region Properties
        public TCommandMap Commands
        {
            get
            {
                if (_Commands == null)
                {
                    _Commands = new TCommandMap();
                }
                return _Commands;
            }
        }
        #endregion

        #region Methods
        public virtual object GetValue([CallerMemberName] string propertyName = null)
        {
            object result;
            if (_PropertyValues.TryGetValue(propertyName, out result)) return result;

            return null;
        }
        public virtual T GetValue<T>([CallerMemberName] string propertyName = null)
        {
            object value = GetValue(propertyName);
            if (value == null) return default(T);
            return (T)value;
        }

        public virtual bool SetValueInternal(object value, [CallerMemberName] string propertyName = null)
        {
            object oldValue = GetValue(propertyName);
            if (!Equals(oldValue, value))
            {
                _PropertyValues[propertyName] = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }
        public virtual bool SetValue<T>(T value, [CallerMemberName] string propertyName = null)
        {
            return SetValueInternal(value, propertyName);
        }

        public virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            App.Current.Dispatcher.Invoke(new Action(()=> {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            }));
        }
        protected void OnPropertyChanged(Expression<Func<object>> expression)
        {
            OnPropertyChanged(PropertyName.For(expression));
        }


        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion



    }
    /// <summary>
    /// Gets property name using lambda expressions.
    /// </summary>
    internal class PropertyName
    {
        public static string For<t>(Expression<Func<t, object>> expression)
        {
            Expression body = expression.Body;
            return GetMemberName(body);
        }

        public static string For(Expression<Func<object>> expression)
        {
            Expression body = expression.Body;
            return GetMemberName(body);
        }

        public static string GetMemberName(Expression expression)
        {
            if (expression is MemberExpression)
            {
                var memberExpression = (MemberExpression)expression;
                if (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    return GetMemberName(memberExpression.Expression) + "." + memberExpression.Member.Name;
                }
                return memberExpression.Member.Name;
            }

            if (expression is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)expression;
                if (unaryExpression.NodeType != ExpressionType.Convert)
                    throw new Exception(string.Format("Cannot interpret member from {0}", expression));

                return GetMemberName(unaryExpression.Operand);
            }

            throw new Exception(string.Format("Could not determine member from {0}", expression));
        }
    }
    public static class TViewModelBaseHelper
    {
        public static void OnPropertyChanged<T>(this T source, Expression<Func<T, object>> expression) where T : TViewModelBase
        {
            string name = PropertyName.For(expression);
            source.OnPropertyChanged(name);
        }
    }
}
