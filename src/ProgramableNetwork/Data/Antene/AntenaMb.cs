using Mafi.Collections;
using Mafi.Core;
using Mafi.Unity.Entities.Static;
using Mafi.Unity.Entities;
using Mafi;
using UnityEngine;
using ProgramableNetwork;
using System;
using Mafi.Unity;

public class AntenaMb : StaticEntityMb, IEntityMbWithRenderUpdate
{
    private Renderer m_render;
    private bool m_hasRender;

    private new Antena Entity => (Antena)base.Entity;

    public void Initialize(Antena display)
    {
        base.Initialize(display);

        if (!gameObject.TryFindChild("influence_fm", out var light))
            throw new NullReferenceException("missing 'influence_fm' object");

        m_render = light.GetComponent<Renderer>();
        if (m_render is null)
            throw new NullReferenceException("missing renderer on 'influence_fm' object");

        m_render = GameObject.Instantiate(m_render.gameObject, m_render.transform.position, m_render.transform.rotation).GetComponent<Renderer>();
        m_render.gameObject.SetActive(false);
        m_hasRender = false;
        GameObject.Destroy(light.gameObject);
    }

    public override void Destroy()
    {
        base.Destroy();
        GameObject.Destroy(m_render.gameObject);
    }

    public void RenderUpdate(GameTime time)
    {
        bool render = Entity.Selected && Entity.DataBand is FMDataBand;
        if (render && !m_hasRender)
        {
            m_hasRender = true;
            m_render.gameObject.SetActive(m_hasRender);
        }
        else if (!render && m_hasRender)
        {
            m_hasRender = false;
            m_render.gameObject.SetActive(m_hasRender);
        }
    }
}