using System.Collections;
using UnityEngine;

namespace ServoMotorSimulator
{
    public class HeuristicControl : UnitySerialPort.SerialServoControllerBase
    {
        protected WaitForSeconds writingRoutineWait;
        protected Coroutine writingCoroutine;

        protected override IEnumerator Start()
        {
            yield return base.Start();

            writingRoutineWait = new WaitForSeconds(writingTime);
            writingCoroutine = StartCoroutine(WritingRoutine());
        }

        protected IEnumerator WritingRoutine()
        {
            while(isActiveAndEnabled)
            {
                Write();

                yield return writingRoutineWait;
            }
        }
    }
}