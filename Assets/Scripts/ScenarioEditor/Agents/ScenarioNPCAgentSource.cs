/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Agents
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Managers;
    using UnityEngine;

    /// <inheritdoc/>
    /// <remarks>
    /// This scenario agent source handles NPC agents
    /// </remarks>
    public class ScenarioNPCAgentSource : ScenarioAgentSource
    {
        /// <inheritdoc/>
        public override string AgentTypeName => "NPCAgent";

        /// <inheritdoc/>
        public override int AgentTypeId => 2;

        /// <inheritdoc/>
        public override List<AgentVariant> AgentVariants { get; } = new List<AgentVariant>();
        
        /// <inheritdoc/>
        public override AgentVariant DefaultVariant { get; set; }

        /// <summary>
        /// Currently dragged agent instance
        /// </summary>
        private GameObject draggedInstance;

        /// <inheritdoc/>
        #pragma warning disable 1998
        public override async Task Initialize()
        {
            var npcVehiclesInSimulation = Web.Config.NPCVehicles;
            foreach (var npcAssetData in npcVehiclesInSimulation)
            {
                var npcVariant = new AgentVariant()
                {
                    source = this,
                    name = npcAssetData.Value.Name,
                    prefab = npcAssetData.Value.prefab
                };
                AgentVariants.Add(npcVariant);
            }

            DefaultVariant = AgentVariants[0];
        }
        #pragma warning restore 1998

        /// <inheritdoc/>
        public override void Deinitialize()
        {
        }

        /// <inheritdoc/>
        public override GameObject GetModelInstance(AgentVariant variant)
        {
            var instance = ScenarioManager.Instance.prefabsPools.GetInstance(variant.prefab);
            if (instance.GetComponent<BoxCollider>() == null)
            {
                var collider = instance.AddComponent<BoxCollider>();
                var b = new Bounds(instance.transform.position, Vector3.zero);
                foreach (Renderer r in instance.GetComponentsInChildren<Renderer>())
                    b.Encapsulate(r.bounds);
                collider.center = b.center - instance.transform.position;
                collider.size = b.size;
            }

            if (instance.GetComponent<Rigidbody>() == null)
            {
                var rigidbody = instance.AddComponent<Rigidbody>();
                rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rigidbody.isKinematic = true;
            }

            return instance;
        }

        /// <inheritdoc/>
        public override ScenarioAgent GetAgentInstance(AgentVariant variant)
        {
            var newGameObject = new GameObject(AgentTypeName);
            newGameObject.transform.SetParent(ScenarioManager.Instance.transform);
            var scenarioAgent = newGameObject.AddComponent<ScenarioAgent>();
            scenarioAgent.Setup(this, variant);
            return scenarioAgent;
        }

        /// <inheritdoc/>
        public override void ReturnModelInstance(GameObject instance)
        {
            ScenarioManager.Instance.prefabsPools.ReturnInstance(instance);
        }

        /// <inheritdoc/>
        public override void DragNewAgent()
        {
            ScenarioManager.Instance.inputManager.StartDraggingElement(this);
        }

        /// <inheritdoc/>
        public override void DragStarted(Vector3 dragPosition)
        {
            draggedInstance = ScenarioManager.Instance.prefabsPools.GetInstance(AgentVariants[0].prefab);
            draggedInstance.transform.SetParent(ScenarioManager.Instance.transform);
            draggedInstance.transform.SetPositionAndRotation(dragPosition, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        }

        /// <inheritdoc/>
        public override void DragMoved(Vector3 dragPosition)
        {
            draggedInstance.transform.position = dragPosition;
        }

        /// <inheritdoc/>
        public override void DragFinished(Vector3 dragPosition)
        {
            var agent = GetAgentInstance(AgentVariants[0]);
            agent.transform.SetPositionAndRotation(draggedInstance.transform.position,
                draggedInstance.transform.rotation);
            ScenarioManager.Instance.prefabsPools.ReturnInstance(draggedInstance);
            draggedInstance = null;
        }

        /// <inheritdoc/>
        public override void DragCancelled(Vector3 dragPosition)
        {
            ScenarioManager.Instance.prefabsPools.ReturnInstance(draggedInstance);
            draggedInstance = null;
        }
    }
}