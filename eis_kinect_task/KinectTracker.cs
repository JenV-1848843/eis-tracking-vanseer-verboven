using Microsoft.Kinect;
using System;

namespace eis_kinect_task
{
    public class KinectTracker : IDisposable
    {
        private KinectSensor m_sensor = null;
        private Skeleton[] m_skeletons = new Skeleton[0];

        public event Action<Skeleton> SkeletonUpdated;

        public KinectSensor Sensor
        {
            get
            {
                return m_sensor;
            }
        }

        public bool IsConnected
        {
            get
            {
                return m_sensor != null;
            }
        }

        public void Start()
        {
            foreach (KinectSensor candidate in KinectSensor.KinectSensors)
            {
                if (m_sensor == null && candidate.Status == KinectStatus.Connected)
                {
                    m_sensor = candidate;
                }
            }

            if (m_sensor != null)
            {
                m_sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                m_sensor.SkeletonStream.Enable();
                m_sensor.SkeletonFrameReady += Sensor_SkeletonFrameReady;
                m_sensor.Start();
            }
        }

        public void Dispose()
        {
            if (m_sensor != null)
            {
                m_sensor.SkeletonFrameReady -= Sensor_SkeletonFrameReady;
                m_sensor.Stop();
            }
        }

        private void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    if (m_skeletons.Length != frame.SkeletonArrayLength)
                    {
                        m_skeletons = new Skeleton[frame.SkeletonArrayLength];
                    }

                    frame.CopySkeletonDataTo(m_skeletons);
                    Skeleton tracked = null;

                    foreach (Skeleton skeleton in m_skeletons)
                    {
                        if (tracked == null && skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            tracked = skeleton;
                        }
                    }

                    Action<Skeleton> handler = SkeletonUpdated;

                    if (handler != null)
                    {
                        handler(tracked);
                    }
                }
            }
        }
    }
}
