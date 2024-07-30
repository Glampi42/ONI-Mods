using ChainErrand.ChainHierarchy;
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
      private static Dictionary<Type, AChainedErrandPack<Workable, ChainedErrand>> _typeMappings = new Dictionary<Type, AChainedErrandPack<Workable, ChainedErrand>>();

      public static AChainedErrandPack<Workable, ChainedErrand> GetChainedErrandPackFromErrand(Workable errand) {
         if(_typeMappings.TryGetValue(errand.GetType(), out var instance))
         {
            return instance;
         }

         throw new InvalidOperationException(Main.debugPrefix + $"No instance of ChainedErrandPack registered for the provided errand of type {errand.GetType()}");
      }


      static ChainedErrandPackRegistry() {
         RegisterAllPacks();
      }

      private static void RegisterAllPacks() {
         // getting all ChainedErrandPack types (all types that extend the AChainedErrandPack base type):
         var types = Assembly.GetCallingAssembly().GetTypes().Where(t => t.IsClass && !t.IsInterface &&
         t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(AChainedErrandPack<,>));

         foreach(var type in types)
         {
            var genericParameters = type.GetGenericArguments();
            try
            {
               Register(genericParameters[0], type);
            }
            catch(Exception ex)
            {
               Console.WriteLine(Main.debugPrefix + $"Failed to register {type.FullName}: {ex.Message}");
            }
         }
      }

      private static void Register(Type errandType, Type chainedErrandPackType) {
         if(!errandType.IsSubclassOf(typeof(Workable)))
            throw new ArgumentException(Main.debugPrefix + $"Type {errandType} is not a valid errand type");
         if(chainedErrandPackType.BaseType == null || !chainedErrandPackType.BaseType.IsGenericType || chainedErrandPackType.BaseType.GetGenericTypeDefinition() != typeof(AChainedErrandPack<,>))
            throw new ArgumentException(Main.debugPrefix + $"Type {chainedErrandPackType.FullName} is not a valid ChainedErrandPack type");

         var instance = Activator.CreateInstance(chainedErrandPackType);
         _typeMappings[errandType] = (AChainedErrandPack<Workable, ChainedErrand>)instance;
      }
   }
}
