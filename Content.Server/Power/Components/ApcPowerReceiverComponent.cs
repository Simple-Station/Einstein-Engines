using Content.Server.Power.NodeGroups;
using Content.Server.Power.Pow3r;
using Content.Shared.Power.Components;

namespace Content.Server.Power.Components
{
    /// <summary>
    ///     Attempts to link with a nearby <see cref="ApcPowerProviderComponent"/>s
    ///     so that it can receive power from a <see cref="IApcNet"/>.
    /// </summary>
    [RegisterComponent]
    public sealed partial class ApcPowerReceiverComponent : SharedApcPowerReceiverComponent
    {
        /// <summary>
        ///     Amount of charge this needs from an APC per second to function.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]

        private float _mainLoad = 5;
        private float _sideLoad;
        
        [DataField("powerLoad")]
        public float Load { get => _mainLoad;
            set { _mainLoad = value; NetworkLoad.DesiredPower = _mainLoad + _sideLoad; }
            }

        [DataField("sidePowerLoad")]
        public float SideLoad
        {
            get => _sideLoad;
            set { _sideLoad = value; NetworkLoad.DesiredPower = _mainLoad + _sideLoad; }
        }

        [ViewVariables(VVAccess.ReadOnly)]
        public float FullLoad => _mainLoad + _sideLoad;

        public ApcPowerProviderComponent? Provider = null;

        /// <summary>
        ///     When false, causes this to appear powered even if not receiving power from an Apc.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool NeedsPower
        {
            get => _needsPower;
            set
            {
                _needsPower = value;
                // Reset this so next tick will do a power update.
                Recalculate = true;
            }
        }

        [DataField("needsPower")]
        private bool _needsPower = true;

        /// <summary>
        ///     When true, causes this to never appear powered.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("powerDisabled")]
        public bool PowerDisabled {
            get => !NetworkLoad.Enabled;
            set => NetworkLoad.Enabled = !value;
        }

        // TODO Is this needed? It forces a PowerChangedEvent when NeedsPower is toggled even if it changes to the same state.
        public bool Recalculate;

        [ViewVariables]
        public PowerState.Load NetworkLoad { get; } = new PowerState.Load
        {
            DesiredPower = 5
        };

        public float PowerReceived => NetworkLoad.ReceivingPower;


        [ViewVariables(VVAccess.ReadOnly)]
        public float SideLoadFraction => _sideLoad > 0 && _needsPower ? MathHelper.Clamp01((NetworkLoad.ReceivingPower - _mainLoad) / _sideLoad) : 1;

        public float LastSideLoadFraction;
    }
}
