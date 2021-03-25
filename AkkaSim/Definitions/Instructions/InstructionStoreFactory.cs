using Akka.Dispatch.SysMsg;

namespace AkkaSim.Definitions.Instructions
{
    public static class InstructionStoreFactory
    {

        public static ICurrentInstructions CreateCurrent(bool debug)
        {
            return debug ? new MessageStore() : new IntegerStore();
        }
        
        public static IFeatureInstructions CreateFeatureStore(bool debug)
        {
            return new FeatureStore();
        }
    }
}