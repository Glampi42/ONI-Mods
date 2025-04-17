using ErrandNotifier.Components;
using PeterHan.PLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ErrandNotifier.ChainedErrandPacks {
   /// <summary>
   /// Interface to retrieve the corresponding NotifiableErrandPack from an errand.
   /// </summary>
   public static class NotifiableErrandPackRegistry {
      private static Dictionary<Type, INotifiableErrandPack> _typeToPackMappings = new();

      public static INotifiableErrandPack GetNotifiableErrandPack(Workable errand) {
         return GetNotifiableErrandPack(errand?.GetType());
      }
      public static INotifiableErrandPack GetNotifiableErrandPack(NotifiableErrand chainedErrand) {
         return GetNotifiableErrandPack(chainedErrand?.GetType());
      }
      public static INotifiableErrandPack GetNotifiableErrandPack(Type type) {
         if(_typeToPackMappings.TryGetValue(type, out var instance))
         {
            return instance;
         }

         throw new InvalidOperationException(Main.debugPrefix + $"No instance of NotifiableErrandPack registered for the provided type {type}");
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
      public static HashSet<INotifiableErrandPack> AllPacks() {
         HashSet<INotifiableErrandPack> result = new();
         foreach(var dictionaryValue in _typeToPackMappings.Values)
         {
            if(!result.Any(pack => pack.GetErrandType() == dictionaryValue.GetErrandType()))
            {
               result.Add(dictionaryValue);
            }
         }

         return result;
      }


      static NotifiableErrandPackRegistry() {
         RegisterAllPacks();
      }

      private static void RegisterAllPacks() {
         // getting all NotifiableErrandPack types (all types that extend the ANotifiableErrandPack base type):
         var types = Main.Assembly.GetTypes().Where(t => t.IsClass && !t.IsInterface &&
         t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(ANotifiableErrandPack<,>));

         foreach(var type in types)
         {
            var genericParameters = type.BaseType.GetGenericArguments();
            Register(genericParameters[0], type);// so that you can access the relating NotifiableErrandPack from an errand
            Register(genericParameters[1], type);// so that you can access the relating NotifiableErrandPack from a NotifiableErrand
         }
      }

      private static void Register(Type type, Type notifiableErrandPackType) {
         if(!type.IsSubclassOf(typeof(Workable)) && !type.IsSubclassOf(typeof(NotifiableErrand)))
            throw new ArgumentException(Main.debugPrefix + $"Type {type} is not valid in current context");
         if(notifiableErrandPackType.BaseType == null || !notifiableErrandPackType.BaseType.IsGenericType || notifiableErrandPackType.BaseType.GetGenericTypeDefinition() != typeof(ANotifiableErrandPack<,>))
            throw new ArgumentException(Main.debugPrefix + $"Type {notifiableErrandPackType.FullName} is not a valid NotifiableErrandPack type");

         var instance = Activator.CreateInstance(notifiableErrandPackType);
         _typeToPackMappings[type] = (INotifiableErrandPack)instance;
      }
   }
}
