using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CommandPattern
{
    public class TimeManager : MonoBehaviour
    {
        private static TimeManager instance;
        public static TimeManager Instance => instance;


        private static float reversalSpeed = 12f;
        private static int recordFrameInterval = 60;
        private int currentFrame = 0;

        private int CurrentFrame => currentFrame;

        private SortedList<int, List<MoveCommand>> snapShots = new();

        // Stores both position and rotation per reversible
        private Dictionary<TimeReversible, (Vector3 pos, Quaternion rot)> StateXFramesAgo = new();

        private bool isReversing = false;
        public bool IsReversing => isReversing;

        private void Awake()
        {
            if(instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }

        void Start()
        {
            var reversibles = FindObjectsByType<TimeReversible>(FindObjectsSortMode.None).ToList();
            foreach (var reversible in reversibles)
            {
                StateXFramesAgo.Add(reversible, (reversible.transform.position, reversible.transform.rotation));
            }
        }

        void Update()
        {
            if (!isReversing)
            {
                HandleRecording();

                if (Input.GetKeyDown(KeyCode.R))
                    StartReversing();
            }
            else
            {
                HandleReversing();

                if (Input.GetKeyUp(KeyCode.R))
                    StopReversing();
            }
        }

        public void StartReversing()
        {
            if (isReversing)
                return;

            if (snapShots.Count == 0)
            {
                currentFrame = 0;
                return;
            }

            foreach (TimeReversible reversible in StateXFramesAgo.Keys)
                reversible.Reverse();

            currentFrame = snapShots.Keys.Last();
            isReversing = true;
        }

        public void StopReversing()
        {
            if (!isReversing)
                return;

            if (snapShots.ContainsKey(currentFrame))
            {
                foreach (var command in snapShots[currentFrame])
                {
                    if (command.currentT != 0)
                        command.Undo();
                }
                currentFrame -= recordFrameInterval;
            }

            List<TimeReversible> copy = StateXFramesAgo.Keys.ToList();
            foreach (var reversible in StateXFramesAgo.Keys)
                reversible.StopReversing();

            foreach (var reversible in copy)
            {
                StateXFramesAgo[reversible] = (reversible.transform.position, reversible.transform.rotation);
            }

            ClearAllSnapshotsAfterFrame(currentFrame);
            isReversing = false;
        }

        private void ClearAllSnapshotsAfterFrame(int frame)
        {
            var keysToRemove = snapShots.Keys.Where(k => k > frame).ToList();
            foreach (var key in keysToRemove)
                snapShots.Remove(key);
        }

        private void HandleRecording()
        {
            currentFrame++;

            if (currentFrame % recordFrameInterval == 0)
            {
                List<(TimeReversible key, Vector3 newPos, Vector3 oldPos, Quaternion newRot, Quaternion oldRot)> changes = new();

                foreach (KeyValuePair<TimeReversible, (Vector3 pos, Quaternion rot)> reversible in StateXFramesAgo)
                {
                    Vector3 oldPos = reversible.Value.pos;
                    Vector3 newPos = reversible.Key.transform.position;
                    Quaternion oldRot = reversible.Value.rot;
                    Quaternion newRot = reversible.Key.transform.rotation;

                    bool posChanged = newPos != oldPos;
                    bool rotChanged = newRot != oldRot;

                    if (posChanged || rotChanged)
                        changes.Add((reversible.Key, newPos, oldPos, newRot, oldRot));
                }

                foreach (var (key, newPos, oldPos, newRot, oldRot) in changes)
                {
                    StateXFramesAgo[key] = (newPos, newRot);
                    MoveCommand move = new MoveCommand(oldPos, newPos, oldRot, newRot, key.gameObject);
                    if (!snapShots.ContainsKey(currentFrame))
                        snapShots[currentFrame] = new List<MoveCommand>();
                    snapShots[currentFrame].Add(move);
                }
            }
        }

        private void HandleReversing()
        {
            if (currentFrame <= 0)
            {
                StopReversing();
                return;
            }

            if (!snapShots.ContainsKey(currentFrame))
                return;

            bool allDone = true;
            float t = reversalSpeed * Time.deltaTime;

            foreach (var command in snapShots[currentFrame])
            {
                if (command.currentT != 0)
                {
                    command.LerpTowardsOld(t);
                    allDone = false;
                }
            }

            if (allDone)
            {
                int previousSnapShotIndex = snapShots.IndexOfKey(currentFrame) - 1;
                currentFrame = previousSnapShotIndex >= 0 ? snapShots.Keys[previousSnapShotIndex] : 0;
            }
        }

        private void OnDrawGizmos()
        {
            foreach (var frame in snapShots)
                foreach (var command in frame.Value)
                    Debug.DrawLine(command.newPos, command.oldPos, Color.yellow);
        }
    }
}