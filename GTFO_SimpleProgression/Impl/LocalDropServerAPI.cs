using DropServer;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Threading.Tasks;
using SimpleProgression.Core;
using System;

namespace SimpleProgression.Impl
{
    internal class LocalDropServerAPI : Il2CppSystem.Object // : IDropServerClientAPI
    {
        public LocalDropServerAPI() : base(ClassInjector.DerivedConstructorPointer<LocalDropServerAPI>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }

        public LocalDropServerAPI(IntPtr ptr) : base(ptr)
        {
            ClassInjector.DerivedConstructorBody(this);
        }

        #region Rundown
        public Task<RundownProgressionResult> RundownProgressionAsync(RundownProgressionRequest request)
        {
            Plugin.L.Warning($"{nameof(LocalDropServerAPI)}: {nameof(RundownProgressionAsync)}: {request.Rundown}");

            var localProgression = LocalProgressionManager.Instance.GetOrCreateLocalProgression(request.Rundown);

            return Task.FromResult(new RundownProgressionResult()
            {
                Rundown = localProgression.ToBaseGameProgression(),
            });
        }

        public Task<ClearRundownProgressionResult> ClearRundownProgressionAsync(ClearRundownProgressionRequest request)
        {
            Plugin.L.Warning($"{nameof(LocalDropServerAPI)}: {nameof(ClearRundownProgressionAsync)}");
            // unused
            return Task.FromResult(new ClearRundownProgressionResult());
        }
        #endregion

        #region Session
        public Task<NewSessionResult> NewSessionAsync(NewSessionRequest request)
        {
            Plugin.L.Warning($"{nameof(LocalDropServerAPI)}: {nameof(NewSessionAsync)}: {request.Rundown} {request.Expedition} {request.SessionId}");

            LocalProgressionManager.Instance.StartNewExpeditionSession(request.Rundown, request.Expedition, request.SessionId);

            return Task.FromResult(new NewSessionResult()
            {
                SessionBlob = $"Chat, is this real?! {request.SessionId}",
            });
        }

        public Task<LayerProgressionResult> LayerProgressionAsync(LayerProgressionRequest request)
        {
            Plugin.L.Warning($"{nameof(LocalDropServerAPI)}: {nameof(LayerProgressionAsync)} Layer: {request.Layer}, State: {request.LayerProgressionState}");

            LocalProgressionManager.Instance.IncreaseLayerProgression(request.Layer, request.LayerProgressionState);

            return Task.FromResult(new LayerProgressionResult()
            {
                SessionBlob = request.SessionBlob,
            });
        }

        public Task<EndSessionResult> EndSessionAsync(EndSessionRequest request)
        {
            var bc = request.BoosterCurrency;
            Plugin.L.Warning($"{nameof(LocalDropServerAPI)}: {nameof(EndSessionAsync)} Success: {request.Success}, BoosterCurrency: M:{bc.Basic}, B:{bc.Advanced}, A:{bc.Specialized}");

            LocalProgressionManager.Instance.EndCurrentExpeditionSession(request.Success);
            LocalBoosterManager.Instance.EndSession(request.BoosterCurrency);

            return Task.FromResult(new EndSessionResult());
        }
        #endregion

        #region BoostersAndVanity
        public Task<GetBoosterImplantPlayerDataResult> GetBoosterImplantPlayerDataAsync(GetBoosterImplantPlayerDataRequest request)
        {
            Plugin.L.Warning($"{nameof(LocalDropServerAPI)}: {nameof(GetBoosterImplantPlayerDataAsync)}");

            return Task.FromResult(new GetBoosterImplantPlayerDataResult()
            {
                Data = LocalBoosterManager.Instance.GetBoosterImplantPlayerData(request.MaxBackendTemplateId),
            });
        }

        public Task<UpdateBoosterImplantPlayerDataResult> UpdateBoosterImplantPlayerDataAsync(UpdateBoosterImplantPlayerDataRequest request)
        {
            Plugin.L.Warning($"{nameof(LocalDropServerAPI)}: {nameof(UpdateBoosterImplantPlayerDataAsync)}");

            return Task.FromResult(new UpdateBoosterImplantPlayerDataResult()
            {
                Data = LocalBoosterManager.Instance.UpdateBoosterImplantPlayerData(request.Transaction),
            });
        }

        public Task<ConsumeBoostersResult> ConsumeBoostersAsync(ConsumeBoostersRequest request)
        {
            Plugin.L.Warning($"{nameof(LocalDropServerAPI)}: {nameof(ConsumeBoostersAsync)}");

            LocalBoosterManager.Instance.ConsumeBoosters(request.SessionBlob);

            return Task.FromResult(new ConsumeBoostersResult()
            {
                SessionBlob = request.SessionBlob,
            });
        }

        public Task<GetInventoryPlayerDataResult> GetInventoryPlayerDataAsync(GetInventoryPlayerDataRequest request)
        {
            Plugin.L.Warning($"{nameof(LocalDropServerAPI)}: {nameof(GetInventoryPlayerDataAsync)}");

            return Task.FromResult(new GetInventoryPlayerDataResult()
            {
                Boosters = LocalBoosterManager.Instance.GetBoosterImplantPlayerData(request.MaxBackendTemplateId),
                VanityItems = LocalVanityItemManager.Instance.GetVanityItemPlayerData(),
            });
        }

        public Task<UpdateVanityItemPlayerDataResult> UpdateVanityItemPlayerDataAsync(UpdateVanityItemPlayerDataRequest request)
        {
            Plugin.L.Warning($"{nameof(LocalDropServerAPI)}: {nameof(UpdateVanityItemPlayerDataAsync)}");

            return Task.FromResult(new UpdateVanityItemPlayerDataResult()
            {
                Data = LocalVanityItemManager.Instance.ProcessTransaction(request.Transaction),
            });
        }
        #endregion

        #region DevDebugStuffs
        public Task<DebugBoosterImplantResult> DebugBoosterImplantAsync(DebugBoosterImplantRequest request)
        {
            Plugin.L.Warning($"{nameof(LocalDropServerAPI)}: {nameof(DebugBoosterImplantAsync)}");
            return Task.FromResult(new DebugBoosterImplantResult());
        }

        public Task<DebugVanityItemResult> DebugVanityItemAsync(DebugVanityItemRequest request)
        {
            Plugin.L.Warning($"{nameof(LocalDropServerAPI)}: {nameof(DebugVanityItemAsync)}");
            //request.AddVanityItemIds

            return Task.FromResult(new DebugVanityItemResult());
        }
        #endregion

        #region OtherLessUsefulMethods
        public Task<AddResult> AddAsync(AddRequest request)
        {
            Plugin.L.Warning($"{nameof(LocalDropServerAPI)}: {nameof(AddAsync)}");
            return Task.FromResult(new AddResult()
            {
                Sum = request.X + request.Y,
            });
        }

        public Task<IsTesterResult> IsTesterAsync(IsTesterRequest request)
        {
            Plugin.L.Warning($"{nameof(LocalDropServerAPI)}: {nameof(IsTesterAsync)}");
            return Task.FromResult(new IsTesterResult()
            {
                IsTester = false,
            });
        }
        #endregion
    }
}
