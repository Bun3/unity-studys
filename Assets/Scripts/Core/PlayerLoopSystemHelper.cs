using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;

namespace zc.core
{
    /// <summary>
    /// copied from PlayerLoopInterface.cs
    /// </summary>
    public static class PlayerLoopSystemHelper {

        private static readonly List<PlayerLoopSystem> insertedSystems = new List<PlayerLoopSystem>();


#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod]
#endif    
        private static void Initialize() {
            // Systems are not automatically removed from the PlayerLoop, so we need to clean up the ones that have been added in play mode, as they'd otherwise
            // keep running when outside play mode, and in the next play mode if we don't have assembly reload turned on.

            Application.quitting += ClearInsertedSystems;
        }

        private static void ClearInsertedSystems ()
        {
            foreach (var playerLoopSystem_ in insertedSystems)
                TryRemoveSystem(playerLoopSystem_.type);

            insertedSystems.Clear();

            Application.quitting -= ClearInsertedSystems;
        }

        private enum InsertType {
            BEFORE,
            AFTER
        }

        /// <summary>
        /// Inserts a new player loop system in the player loop, just after another system.
        /// </summary>
        /// <param name="inNewSystemMarker">Type marker for the new system.</param>
        /// <param name="inNewSystemUpdate">Callback that will be called each frame after insertAfter.</param>
        /// <param name="inSertAfter">The subsystem to insert the system after.</param>
        public static void InsertSystemAfter(Type inNewSystemMarker, PlayerLoopSystem.UpdateFunction inNewSystemUpdate, Type inSertAfter) {
            var playerLoopSystem_ = new PlayerLoopSystem {type = inNewSystemMarker, updateDelegate = inNewSystemUpdate};
            InsertSystemAfter(playerLoopSystem_, inSertAfter);
        }

        /// <summary>
        /// Inserts a new player loop system in the player loop, just before another system.
        /// </summary>
        /// <param name="inNewSystemMarker">Type marker for the new system.</param>
        /// <param name="inNewSystemUpdate">Callback that will be called each frame before insertBefore.</param>
        /// <param name="inSertBefore">The subsystem to insert the system before.</param>
        public static void InsertSystemBefore(Type inNewSystemMarker, PlayerLoopSystem.UpdateFunction inNewSystemUpdate, Type inSertBefore) {
            var playerLoopSystem_ = new PlayerLoopSystem {type = inNewSystemMarker, updateDelegate = inNewSystemUpdate};
            InsertSystemBefore(playerLoopSystem_, inSertBefore);
        }

        /// <summary>
        /// Inserts a new player loop system in the player loop, just after another system.
        /// </summary>
        /// <param name="inToInsert">System to insert. Needs to have updateDelegate and Type set.</param>
        /// <param name="inSertAfter">The subsystem to insert the system after</param>
        public static void InsertSystemAfter(PlayerLoopSystem inToInsert, Type inSertAfter) {
            if (inToInsert.type == null)
                throw new ArgumentException("The inserted player loop system must have a marker type!", nameof(inToInsert.type));
            if (inToInsert.updateDelegate == null)
                throw new ArgumentException("The inserted player loop system must have an update delegate!", nameof(inToInsert.updateDelegate));
            if (inSertAfter == null)
                throw new ArgumentNullException(nameof(inSertAfter));

            var rootSystem_ = PlayerLoop.GetCurrentPlayerLoop();

            InsertSystem(ref rootSystem_, inToInsert, inSertAfter, InsertType.AFTER, out var couldInsert_);
            if (!couldInsert_) {
                throw new ArgumentException($"When trying to insert the type {inToInsert.type.Name} into the player loop after {inSertAfter.Name}, " +
                                            $"{inSertAfter.Name} could not be found in the current player loop!");
            }

            insertedSystems.Add(inToInsert);
            PlayerLoop.SetPlayerLoop(rootSystem_);
        }

        /// <summary>
        /// Inserts a new player loop system in the player loop, just before another system.
        /// </summary>
        /// <param name="inToInsert">System to insert. Needs to have updateDelegate and Type set.</param>
        /// <param name="inSertBefore">The subsystem to insert the system before</param>
        public static void InsertSystemBefore(PlayerLoopSystem inToInsert, Type inSertBefore) {
            if (inToInsert.type == null)
                throw new ArgumentException("The inserted player loop system must have a marker type!", nameof(inToInsert.type));
            if (inToInsert.updateDelegate == null)
                throw new ArgumentException("The inserted player loop system must have an update delegate!", nameof(inToInsert.updateDelegate));
            if (inSertBefore == null)
                throw new ArgumentNullException(nameof(inSertBefore));

            var rootSystem_ = PlayerLoop.GetCurrentPlayerLoop();
            InsertSystem(ref rootSystem_, inToInsert, inSertBefore, InsertType.BEFORE, out var couldInsert_);
            if (!couldInsert_) {
                throw new ArgumentException($"When trying to insert the type {inToInsert.type.Name} into the player loop before {inSertBefore.Name}, " +
                                            $"{inSertBefore.Name} could not be found in the current player loop!");
            }

            insertedSystems.Add(inToInsert);
            PlayerLoop.SetPlayerLoop(rootSystem_);
        }

        /// <summary>
        /// Tries to remove a system from the PlayerLoop. The first system found that has the same type identifier as the supplied one will be removed.
        /// </summary>
        /// <param name="inType">Type identifier of the system to remove</param>
        /// <returns></returns>
        public static bool TryRemoveSystem(Type inType) {
            if (inType == null)
                throw new ArgumentNullException(nameof(inType), "Trying to remove a null type!");

            var currentSystem_ = PlayerLoop.GetCurrentPlayerLoop();
            var couldRemove_ = TryRemoveTypeFrom(ref currentSystem_, inType);
            PlayerLoop.SetPlayerLoop(currentSystem_);
            return couldRemove_;
        }

        private static bool TryRemoveTypeFrom(ref PlayerLoopSystem inCurrentSystem, Type inType) {
            var subSystems_ = inCurrentSystem.subSystemList;
            if (subSystems_ == null)
                return false;

            for (int i_ = 0; i_ < subSystems_.Length; i_++) {
                if (subSystems_[i_].type == inType) {
                    var newSubSystems_ = new PlayerLoopSystem[subSystems_.Length - 1];

                    Array.Copy(subSystems_, newSubSystems_, i_);
                    Array.Copy(subSystems_, i_ + 1, newSubSystems_, i_, subSystems_.Length - i_ - 1);

                    inCurrentSystem.subSystemList = newSubSystems_;

                    return true;
                }

                if (TryRemoveTypeFrom(ref subSystems_[i_], inType))
                    return true;
            }

            return false;
        }

        public static PlayerLoopSystem CopySystem(PlayerLoopSystem inSystem) {
            // PlayerLoopSystem is a struct.
            var copy_ = inSystem;

            // but the sub system list is an array.
            if (inSystem.subSystemList != null) {
                copy_.subSystemList = new PlayerLoopSystem[inSystem.subSystemList.Length];
                for (int i_ = 0; i_ < copy_.subSystemList.Length; i_++) {
                    copy_.subSystemList[i_] = CopySystem(inSystem.subSystemList[i_]);
                }
            }

            return copy_;
        }

        private static void InsertSystem(ref PlayerLoopSystem inCurrentLoopRecursive, PlayerLoopSystem inToInsert, Type inSertTarget, InsertType inSertType,
            out bool inCouldInsert) {
            var currentSubSystems_ = inCurrentLoopRecursive.subSystemList;
            if (currentSubSystems_ == null) {
                inCouldInsert = false;
                return;
            }

            int indexOfTarget_ = -1;
            for (int i_ = 0; i_ < currentSubSystems_.Length; i_++) {
                if (currentSubSystems_[i_].type == inSertTarget) {
                    indexOfTarget_ = i_;
                    break;
                }
            }

            if (indexOfTarget_ != -1) {
                var newSubSystems_ = new PlayerLoopSystem[currentSubSystems_.Length + 1];

                var insertIndex_ = inSertType == InsertType.BEFORE ? indexOfTarget_ : indexOfTarget_ + 1;

                for (int i_ = 0; i_ < newSubSystems_.Length; i_++) {
                    if (i_ < insertIndex_)
                    {
                        newSubSystems_[i_] = currentSubSystems_[i_];
                    }
                    else if (i_ == insertIndex_) {
                        newSubSystems_[i_] = inToInsert;
                    }
                    else {
                        newSubSystems_[i_] = currentSubSystems_[i_ - 1];
                    }
                }

                inCouldInsert = true;
                inCurrentLoopRecursive.subSystemList = newSubSystems_;
            }
            else {
                for (var i_ = 0; i_ < currentSubSystems_.Length; i_++) {
                    var subSystem_ = currentSubSystems_[i_];
                    InsertSystem(ref subSystem_, inToInsert, inSertTarget, inSertType, out var couldInsertInInner_);
                    if (couldInsertInInner_) {
                        currentSubSystems_[i_] = subSystem_;
                        inCouldInsert = true;
                        return;
                    }
                }

                inCouldInsert = false;
            }
        }
    
        /// <summary>
        /// Utility to get a string representation of the current player loop.
        /// </summary>
        /// <returns>String representation of the current player loop system.</returns>
        public static string CurrentLoopToString()
        {
            return PrintSystemToString(PlayerLoop.GetCurrentPlayerLoop());
        }

        private static string PrintSystemToString(PlayerLoopSystem inPlayerLoopSystem) {
            List<(PlayerLoopSystem, int)> systems_ = new List<(PlayerLoopSystem, int)>();

            AddRecursively(inPlayerLoopSystem, 0);
            void AddRecursively(PlayerLoopSystem inSystem, int inDepth)
            {
                systems_.Add((inSystem, inDepth));
                if (inSystem.subSystemList != null)
                    foreach (var subsystem_ in inSystem.subSystemList)
                        AddRecursively(subsystem_, inDepth + 1);
            }

            StringBuilder sb_ = new StringBuilder();
            sb_.AppendLine("Systems");
            sb_.AppendLine("=======");
            foreach (var (system_, depth_) in systems_)
            {
                // root system has a null type, all others has a marker type.
                Append($"System Type: {system_.type?.Name ?? "NULL"}");

                // This is a C# delegate, so it's only set for functions created on the C# side.
                Append($"Delegate: {system_.updateDelegate}");

                // This is a pointer, probably to the function getting run internally. Has long values (like 140700263204024) for the builtin ones concrete ones,
                // while the builtin grouping functions has 0. So UnityEngine.PlayerLoop.Update has 0, while UnityEngine.PlayerLoop.Update.ScriptRunBehaviourUpdate
                // has a concrete value.
                Append($"Update Function: {system_.updateFunction}");

                // The loopConditionFunction seems to be a red herring. It's set to a value for only UnityEngine.PlayerLoop.FixedUpdate, but setting a different
                // system to have the same loop condition function doesn't seem to do anything
                Append($"Loop Condition Function: {system_.loopConditionFunction}");

                // null rather than an empty array when it's empty.
                Append($"{system_.subSystemList?.Length ?? 0} subsystems");

                void Append(string inStr)
                {
                    for (int i_ = 0; i_ < depth_; i_++)
                        sb_.Append("  ");
                    sb_.AppendLine(inStr);
                }
            }

            return sb_.ToString();
        }

        // [MenuItem("Test/Output current state to file")]
        private static void OutputCurrentStateToFile()
        {
            var str_ = PrintSystemToString(PlayerLoop.GetCurrentPlayerLoop());

            Debug.Log(str_);
            File.WriteAllText("playerLoopInterfaceOutput.txt", str_);
        }
    }
}
