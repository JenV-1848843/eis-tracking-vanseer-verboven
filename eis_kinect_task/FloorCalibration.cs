using Microsoft.Kinect;
using Microsoft.Samples.Kinect.ControlsBasics;

namespace eis_kinect_task
{
    public class FloorCalibration
    {
        private const int RequiredPoints = 4;

        private readonly PartialCalibrationClass m_calibration;
        private readonly System.Windows.Point[] m_targets;
        private int m_pointCount = 0;

        public FloorCalibration(KinectSensor sensor, System.Windows.Point[] targets)
        {
            m_calibration = new PartialCalibrationClass(sensor);
            m_targets = targets;
        }

        public int PointCount
        {
            get
            {
                return m_pointCount;
            }
        }

        public bool IsCalibrated
        {
            get
            {
                return m_pointCount == RequiredPoints;
            }
        }

        public System.Windows.Point[] Targets
        {
            get
            {
                return m_targets;
            }
        }

        public void AddPoint(SkeletonPoint position)
        {
            if (!IsCalibrated)
            {
                m_calibration.addCalibrationPoint(m_targets[m_pointCount], position);
                m_pointCount++;

                if (IsCalibrated)
                {
                    m_calibration.calibrate();
                }
            }
        }

        public System.Windows.Point ToScreen(SkeletonPoint position)
        {
            return m_calibration.kinectToProjectionPoint(position);
        }
    }
}
