using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Extensions
{
    public static class DebugExtensions
    {
        [Conditional("UNITY_EDITOR")]
        public static void TrimNameStart(this EntityManager em, NativeArray<Entity> entities, string prefix)
        {
            for (int i = 0; i < entities.Length; i++)
            {
                TrimNameStart(em, entities[i], prefix);
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void TrimNameStart(this EntityManager em, Entity entity, string prefix)
        {
            em.SetName(entity, em.GetName(entity).TrimStart(prefix));
        }

        [Conditional("UNITY_EDITOR")]
        public static void PrependName(this EntityManager em, NativeArray<Entity> entities, string prefix)
        {
            for (int i = 0; i < entities.Length; i++)
            {
                PrependName(em, entities[i], prefix);
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void PrependName(this EntityManager em, Entity entity, string prefix)
        {
            var currentName = em.GetName(entity);
            if (!currentName.StartsWith(prefix))
            {
                em.SetName(entity, prefix + em.GetName(entity));
            }
        }
    }
}

