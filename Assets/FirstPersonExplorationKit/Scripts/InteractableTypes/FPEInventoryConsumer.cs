using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whilefun.FPEKit
{

    //
    // FPEInventoryConsumer
    // This script contains an abstract class on which Inventory Consumer scripts must be 
    // based. When implemented, a Consumer will fire events when the associated consumable 
    // Inventory item is consumed. For example, when consuming the demoApple, the 
    // demoInventoryConsumer drops an apple core and plays a sound.
    //
    // Copyright 2021 While Fun Games
    // http://whilefun.com
    //
    public abstract class FPEInventoryConsumer : MonoBehaviour
    {

        private bool consumptionStarted = false;
        protected bool ConsumptionStarted {  get { return consumptionStarted; } }
        private bool consumptionCompleted = false;

        public virtual void Update()
        {

            if (consumptionCompleted)
            {
                Destroy(gameObject);
            }

        }

        /// <summary>
        /// A one-way 'fire and forget' call to consume the item. Once called, this class will take care of executing required scripts and destroying the item once complete.
        /// </summary>
        public virtual void consumeItem()
        {
            consumptionStarted = true;
        }

        /// <summary>
        /// This function must be called by child classes of FPEInventoryConsumer so that the object is cleaned up.
        /// </summary>
        public void finishConsumption()
        {
            consumptionCompleted = true;
        }
       
    }

}