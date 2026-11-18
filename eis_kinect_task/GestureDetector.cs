using Microsoft.Kinect;
using System;

namespace eis_kinect_task
{
    public class GestureDetector
    {
        private readonly int m_holdFrames;
        private Symbol m_heldSymbol = Symbol.Empty;
        private int m_heldFrames = 0;
        private bool m_latched = false;

        public GestureDetector(int holdFrames)
        {
            m_holdFrames = holdFrames;
        }

        public Symbol HeldSymbol
        {
            get
            {
                return m_heldSymbol;
            }
        }

        public double Progress
        {
            get
            {
                double progress = 0.0;

                if (m_heldSymbol != Symbol.Empty && !m_latched)
                {
                    progress = Math.Min(1.0, (double)m_heldFrames / m_holdFrames);
                }

                return progress;
            }
        }

        public Symbol Update(Skeleton skeleton)
        {
            Symbol pose = Symbol.Empty;
            Symbol completed = Symbol.Empty;

            if (skeleton != null)
            {
                pose = DetectPose(skeleton);
            }

            if (pose == Symbol.Empty)
            {
                Reset();
            }
            else if (pose != m_heldSymbol)
            {
                m_heldSymbol = pose;
                m_heldFrames = 1;
                m_latched = false;
            }
            else
            {
                m_heldFrames++;
            }

            if (!m_latched && pose != Symbol.Empty && m_heldFrames >= m_holdFrames)
            {
                completed = pose;
                m_latched = true;
            }

            return completed;
        }

        public void Reset()
        {
            m_heldSymbol = Symbol.Empty;
            m_heldFrames = 0;
            m_latched = false;
        }

        private static Symbol DetectPose(Skeleton skeleton)
        {
            SkeletonPoint handLeft = skeleton.Joints[JointType.HandLeft].Position;
            SkeletonPoint handRight = skeleton.Joints[JointType.HandRight].Position;
            SkeletonPoint head = skeleton.Joints[JointType.Head].Position;

            bool rightUp = handRight.Y > head.Y;
            bool leftUp = handLeft.Y > head.Y;
            Symbol pose = Symbol.Empty;

            if (rightUp && !leftUp)
            {
                pose = Symbol.X;
            }
            else if (leftUp && !rightUp)
            {
                pose = Symbol.O;
            }

            return pose;
        }
    }
}
