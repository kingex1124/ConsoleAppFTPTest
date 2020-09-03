using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// 
    /// </summary>
    public class UnityContainer
    {
        /// <summary>
        /// 所有註冊的物件
        /// </summary>
        static Dictionary<Type, List<Type>> Maps = new Dictionary<Type, List<Type>>();
        //static Dictionary<Type, List<Type>> Maps = new Dictionary<Type, List<Type>>();

        /// <summary>
        /// 註冊
        /// </summary>
        /// <typeparam name="TAbstractType"></typeparam>
        /// <typeparam name="TConcreteType"></typeparam>
        public static void Register<TAbstractType, TConcreteType>()
        {
            if (Maps.ContainsKey(typeof(TAbstractType)))
            {
                var list = Maps[typeof(TAbstractType)];
                var insance = list.Where(o => o == typeof(TConcreteType));

                if (insance.Any())
                    return;
                else
                    Maps[typeof(TAbstractType)].Add(typeof(TConcreteType));
                // Maps[typeof(TAbstractType)].Add(typeof(TConcreteType));
                return;
            }

            Maps.Add(typeof(TAbstractType), new List<Type>() { typeof(TConcreteType) });
        }

        /// <summary>
        /// 解析(實體方法、變數多於抽象時使用)
        /// </summary>
        /// <typeparam name="TAbstractType"></typeparam>
        /// <typeparam name="TConcreteType"></typeparam>
        /// <returns></returns>
        public static TConcreteType Resolve<TAbstractType, TConcreteType>(object[] parm)
        {
            var list = Maps[typeof(TAbstractType)];
            var insance = list.Where(o => o == typeof(TConcreteType)).FirstOrDefault();

            if (insance != null)
            {
                Type fooConcreteType = insance; //.Find(o => o == typeof(TInstanceType));

                Object instance = Activator.CreateInstance(fooConcreteType, parm);
                return (TConcreteType)instance;
            }
            else
                return default;
        }
    }
}
