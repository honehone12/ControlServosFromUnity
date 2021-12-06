using System.Collections;
using UnityEngine;

namespace ServoMotorSimulator
{
    public class DemoControl : UnitySerialPort.SerialServoControllerBase
    {
        [SerializeField]
        [Range(0, 180)]
        protected byte value = 90;

        protected override IEnumerator Start()
        {
            yield return base.Start();

            byte diff = 
                value >= ServoConstants.VALUE_STOPPING ? 
                (byte)(value - ServoConstants.VALUE_STOPPING) : (byte)(ServoConstants.VALUE_STOPPING - value);


            simulatorsList.ForEach(
                (sim) => sim.OnLimitationReached += (ref LimitationCheckInfo info) =>
                {
                    if (sim.Value > ServoConstants.VALUE_STOPPING)
                    {
                        sim.Value = (byte)(ServoConstants.VALUE_STOPPING - diff);
                    }
                    else
                    {
                        sim.Value = (byte)(ServoConstants.VALUE_STOPPING + diff);
                    }

                    Write();
                }
            );

            simulatorsList.ForEach(
                (sim) => sim.Value = value
            );

            Write();
        }
    }
}