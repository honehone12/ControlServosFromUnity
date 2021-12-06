using UnityEngine;
using ServoMotorSimulator.Extensions;

namespace ServoMotorSimulator
{
    public delegate void ServoSimulatorEvent();

    public class ServoSimulator : MonoBehaviour
    {
        [SerializeField]
        private SimMode mode;
        [Header("Transforms")]
        [SerializeField]
        [Tooltip("Serialize if child(0)'s Transform is not target.")]
        private Transform targetTF;
        [Header("Profile")]
        [SerializeField]
        private ServoProfile profile;
        [Header("Actuation")]
        [SerializeField]
        private SimAxis axis;
        [SerializeField]
        private SimDirection direction;
        [Header("Limitation")]
        [SerializeField]
        private SimLimitation limitation;
        [Header("Value")]
        [Range(0, 180)]
        [SerializeField]
        private byte value = 90;

        private float currentTheta;
        private float lastDelta;
        private float dumperEndTime;

        public event LimitationCheckEvent OnLimitationReached;
        public event ServoSimulatorEvent OnDumperWorked;

        public SimMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        public byte Value
        {
            get { return value; }
            set 
            {
                this.value = 
                    value > ServoConstants.VALUE_MAX ? ServoConstants.VALUE_MAX : value;
            }
        }

        public float FeedBack
        {
            get
            {
                return targetTF.GetLocalTheta(axis);
            }
        }

        public bool LimitationCheck(out LimitationCheckInfo info)
        {
            info = new LimitationCheckInfo();
            info.theta = targetTF.GetLocalTheta(axis);
            info.theta = info.theta > 180.0f && limitation.min < 0.0f ? info.theta - 360.0f : info.theta;
            SimDirection nextDir = value.ToSimDirection().Blend(direction);
            info.delta = profile.GetIterationSpeed(value) * direction.ToFloat();
            float nextTheta = mode switch
            {
                SimMode.Virtual => info.theta + info.delta, 
                SimMode.SerialViz => info.theta + info.delta * 10.0f,
                _ => info.theta + info.delta,
            };

            info.result = !(
                (nextTheta <= limitation.min && nextDir == SimDirection.Reversal) ||
                (nextTheta >= limitation.max && nextDir == SimDirection.Forward)
            );

            if (!info.result)
            {
                OnLimitationReached?.Invoke(ref info);

                //Debug.LogWarningFormat(
                //    "limitation {1} => {1} : {2} of {3}",
                //    info.theta, nextTheta, name, transform.root.name
                //);
            }

            return info.result;
        }

        public void SetAngle(float angle)
        {
            if(mode == SimMode.SerialViz)
            {
                targetTF.SetLocalTheta(axis, angle);
            }
        }

        private void Awake()
        {
            if(!profile)
            {
                Debug.LogError("please serialize profile before play.");
            }

            targetTF = targetTF ? targetTF : transform.GetChild(0);
        }

        private void Start()
        {
            if(mode == SimMode.Virtual)
            {
                currentTheta = targetTF.GetLocalTheta(axis);
            }
        }

        private float Dumper(float velocity)
        {
            float now = Time.time;
            if (velocity * lastDelta < 0.0f)
            {
                dumperEndTime = now + profile.DumperSec;
                OnDumperWorked?.Invoke();
                //Debug.LogFormat("Dumper will work until {0}.", dumperEndTime);
            }

            return dumperEndTime > now ? 0.0f : velocity;
        }

        private void FixedUpdate()
        {
            if(mode == SimMode.Virtual)
            {
                if(LimitationCheck(out _))
                {
                    float delta = profile.GetIterationSpeed(value) * direction.ToFloat();
                    delta = Dumper(delta);
                    currentTheta = (currentTheta + delta) % 360.0f;
                    targetTF.SetLocalTheta(axis, currentTheta);
                    lastDelta = delta;
                }
            }
        }
    }
}