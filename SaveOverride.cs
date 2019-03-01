using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;
using ModAPI.Attributes;

namespace BuilderCore
{
    public class LoadOverride : Clock
    {
        [Priority(10000)]
        protected override void Awake()
        {

            Invoke("DelayedStart",2);
            base.Awake();

        }

        public void DelayedStart()
        {
            try
            {
             Core.LoadPlacedBuildings();

            }
            catch (Exception e)
            {
                ModAPI.Log.Write(e.ToString());
            }
            
        }
     
   
    }

    

    public class SaveOverride : PlayerStats
    {
        [Priority(10000)]
        public override void JustSave()
        {
            base.JustSave();
            Core.SavePlacedBuildings();
        }
    }
}
