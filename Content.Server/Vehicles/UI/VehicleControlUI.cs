// /Content.Client/Vehicles/UI/VehicleControlUI.cs
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Vehicles.UI
{
    public sealed class VehicleControlUI : Control
    {

        //TODO insert sad face
        public VehicleControlUI()
        {
            var enterButton = new Button { Text = "Enter Vehicle" };
            enterButton.OnPressed += _ => EnterVehicle();
            AddChild(enterButton);

            var exitButton = new Button { Text = "Exit Vehicle" };
            exitButton.OnPressed += _ => ExitVehicle();
            AddChild(exitButton);
        }

        private void EnterVehicle()
        {


            private void ExitVehicle()
            {

            }
        }
    }
