using Fixtures;
using OscController;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using VLB;
using static ContentLightingMetaData;
using static HouseBuilder.HouseBuilderConstants;

namespace HouseBuilder
{
    public class Staging
    {
        public ContentStagingMetaData ContentStagingMetaDataRef { get; private set; }
        public InteractionGameObject Interaction{ get; private set; }

        public HouseBuilderConstants.StagingData StagingDataRef { get; private set; }

        public void Setup(InteractionGameObject inter, ContentStagingMetaData metaData, HouseBuilderConstants.StagingData data)
        {
            Interaction = inter;
            ContentStagingMetaDataRef = metaData;
            ContentStagingMetaDataRef.CollectAllData(inter.RootGameObject);

            StagingDataRef = data;
        }
    }
}

