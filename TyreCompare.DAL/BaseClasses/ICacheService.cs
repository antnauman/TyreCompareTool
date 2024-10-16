﻿namespace TyreCompare.DAL.BaseClasses;

public interface ICacheService
{
    T Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    void Delete<T>(string key);
}