﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Puppet;
using Puppet.Core;
using Puppet.Utils;
using Puppet.Utils.Threading;
using Puppet.Service;
using Puppet.Core.Model;
using Puppet.Core.Manager;
using System;


public class PuApp : Singleton<PuApp>
{
	public bool changingScene;
    public PuSetting setting;

    private int sleepTimeout;

    List<KeyValuePair<EMessage, string>> listMessage = new List<KeyValuePair<EMessage, string>>();

    protected override void Init() { }

    public void StartApplication(Action<float, string> onLoadConfig) 
    {
#if UNITY_WEBPLAYER
        if (!Application.isEditor && Application.isWebPlayer)
        { 
            //if (!Security.PrefetchSocketPolicy(AppConfig.SocketUrl, AppConfig.SocketPort, 999))
            if (!Security.PrefetchSocketPolicy("210.245.94.106", 9933, 999))
                Debug.LogError("Security Exception. Policy file load failed!");
            else
                Debug.LogWarning("Security Good. Policy file load success!");
        }
#endif

        sleepTimeout = Screen.sleepTimeout;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        setting = new PuSetting(onLoadConfig);

        //PingManager.Instance.Load();

        PuMain.Setting.Threading.QueueOnMainThread(() =>
        {
            PuMain.Dispatcher.onWarningUpgrade += Dispatcher_onWarningUpgrade;
            PuMain.Dispatcher.onDailyGift += Dispatcher_onDailyGift;
            PuMain.Dispatcher.onNoticeMessage += Dispatcher_onNoticeMessage;
        });

        SocialService.SocialStart();
    }

    void Dispatcher_onNoticeMessage(EMessage type, string message)
    {
        PuMain.Setting.Threading.QueueOnMainThread(() =>
        {
            string title = 
                type == EMessage.Message ? "Thông báo" : 
                type == EMessage.Warning ? "Cảnh báo" :
                type == EMessage.Error ? "Lỗi" : type.ToString();
            DialogService.Instance.ShowDialog(new DialogMessage(title, message, null));
        });
    }

	void Dispatcher_onDailyGift(DataDailyGift obj)
	{
        this.dailyGift = obj;
        PuMain.Setting.Threading.QueueOnMainThread(() =>
        {
            ExecuteFuntion(.5f, () =>
            {
                DialogService.Instance.ShowDialog(new DialogPromotion(obj));
            });
        });
	}

    void Dispatcher_onWarningUpgrade(EUpgrade type, string message, string market)
    {
        if (type == EUpgrade.ForceUpdate || type == EUpgrade.MaybeUpdate)
        {
            PuMain.Setting.Threading.QueueOnMainThread(() =>
            {
                DialogService.Instance.ShowDialog(new DialogConfirm("Kiểm tra phiên bản", message, delegate(bool? click)
                {
                    if (click == true || type == EUpgrade.ForceUpdate)
                        Application.OpenURL(market);
                }));
            });
        }
    }
	
    void FixedUpdate()
    {
        if (setting != null)
            setting.Update();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (setting != null)
            setting.OnApplicationPause(pauseStatus);
    }

    void OnApplicationQuit()
    {
        Screen.sleepTimeout = sleepTimeout;
        if (setting != null)
            setting.OnApplicationQuit();
    }

    public void BackScene()
    {
        Puppet.API.Client.APIGeneric.BackScene((bool status, string message) => {
            if (!status)
                Logger.Log(message);
        });
    }

    public void ExecuteFuntion(float delayTime, System.Action callback)
    {
        StartCoroutine(_DelayFunction(delayTime, callback));
    }

    IEnumerator _DelayFunction(float delayTime, System.Action callback)
    {
        yield return new WaitForSeconds(delayTime);
        if (callback != null)
            callback();
    }

    public void RequestInviteApp(string message)
    {
        SocialType type = SocialType.Facebook;
        Puppet.Service.SocialService.AppRequest(type, message, null, null, (status, requestIds) => 
        {
            if(status)
            {
                Puppet.API.Client.APIGeneric.SaveRequestFB(SocialService.GetSocialNetwork(type).UserId, requestIds, (saveStatus, saveMessage) =>
                {
                    string responseMessage = saveStatus ? "Bạn đã gửi lời mới thành công." : saveMessage;
                    DialogService.Instance.ShowDialog(new DialogMessage("Gửi lời mời.", responseMessage, null));
                });
            }
            else
            {
                DialogService.Instance.ShowDialog(new DialogMessage("Gửi lời mời.", "Không thể gửi lời mới cho bạn bè", null));
            }
        });
    }

    public void GetImage(string path, Action<Texture2D> callback)
    {
        PuDLCache.Instance.HttpRequestCache(path, (status, error, bytes) =>
        {
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
            if(status)
            {
                texture.LoadImage(bytes);
            }
            else
            {
                Logger.Log("Get Images from path '{0}' error: {1}", path, error);
            }

            if (callback != null)
                callback(texture);
        });
    }

    public DataDailyGift dailyGift { get; set; }
}
