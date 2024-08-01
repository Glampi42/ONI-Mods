using ChainErrand.ChainHierarchy;
using PeterHan.PLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChainErrand.ChainedErrandPacks {
   /// <summary>
   /// Interface to retrieve the corresponding ChainedErrandPack from an errand.
   /// </summary>
   public static class ChainedErrandPackRegistry {
      private static Dictionary<Type, IChainedErrandPack> _typeToPackMappings = new();

      public static IChainedErrandPack GetChainedErrandPack(Workable errand) {
         return GetChainedErrandPack(errand?.GetType());
      }
      public static IChainedErrandPack GetChainedErrandPack(ChainedErrand chainedErrand) {
         return GetChainedErrandPack(chainedErrand?.GetType());
      }
      public static IChainedErrandPack GetChainedErrandPack(Type type) {
         if(_typeToPackMappings.TryGetValue(type, out var instance))
         {
            return instance;
         }

         throw new InvalidOperationException(Main.debugPrefix + $"No instance of ChainedErrandPack registered for the provided type {type}");
      }

      /// <summary>
      /// Retrieves all errand types that can/should have a ChainedErrandPack and can be added to a chain.
      /// </summary>
      /// <returns>The types.</returns>
      public static IEnumerable<Type> AllErrandTypes() {
         return _typeToPackMappings.Keys.Where(type => type.IsSubclassOf(typeof(Workable)));
      }
      /// <summary>
      /// Retrieves all registered ChainedErrandPacks.
      /// </summary>
      /// <returns>The packs.</returns>
      public static HashSet<IChainedErrandPack> AllPacks() {
         HashSet<IChainedErrandPack> result = new();
         foreach(var dictionaryValue in _typeToPackMappings.Values)
         {
            if(!result.Any(pack => pack.GetErrandType() == dictionaryValue.GetErrandType()))
            {
               result.Add(dictionaryValue);
            }
         }

         return result;
      }


      static ChainedErrandPackRegistry() {
         RegisterAllPacks();
      }

      private static void RegisterAllPacks() {
         Debug.Log("$$$RegisterAllPacks");
         // getting all ChainedErrandPack types (all types that extend the AChainedErrandPack base type):
         var types = Main.Assembly.GetTypes().Where(t => t.IsClass && !t.IsInterface &&
         t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(AChainedErrandPack<,>));

         foreach(var type in types)
         {
            var genericParameters = type.BaseType.GetGenericArguments();
            Debug.Log("Registered " + type.ToString());
            Debug.Log("Key1: " + genericParameters[0].ToString());
            Debug.Log("Key2: " + genericParameters[1].ToString());
            Register(genericParameters[0], type);// so that you can access the relating ChainedErrandPack from an errand
            Register(genericParameters[1], type);// so that you can access the relating ChainedErrandPack from a ChainedErrand
         }
      }

      private static void Register(Type type, Type chainedErrandPackType) {
         if(!type.IsSubclassOf(typeof(Workable)) && !type.IsSubclassOf(typeof(ChainedErrand)))
            throw new ArgumentException(Main.debugPrefix + $"Type {type} is not valid in current context");
         if(chainedErrandPackType.BaseType == null || !chainedErrandPackType.BaseType.IsGenericType || chainedErrandPackType.BaseType.GetGenericTypeDefinition() != typeof(AChainedErrandPack<,>))
            throw new ArgumentException(Main.debugPrefix + $"Type {chainedErrandPackType.FullName} is not a valid ChainedErrandPack type");

         var instance = Activator.CreateInstance(chainedErrandPackType);
         Debug.Log("Who?: " + (instance.GetType().FullName));
         _typeToPackMappings[type] = (IChainedErrandPack)instance;
      }
   }
}
