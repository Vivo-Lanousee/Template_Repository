using R3;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;


namespace Common
{
    /// <summary>
    /// グリッドを使う時、Reactiveを使いたい場合。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReactiveGrid<T> : IDisposable, IEnumerable<T>
    {
        private readonly T[,] data;
        private readonly Subject<(int x, int y, T value)> onChangeSubject = new();
        private bool isDisposed = false;
        public int width { get; }
        public int height { get; }
        public Observable<(int x, int y, T value)> OnValueChanged => onChangeSubject;

        public ReactiveGrid(int _width, int _height)
        {
            width = _width;
            height = _height;
            data = new T[_width, _height];
        }

        public T this[int x, int y]
        {
            get => data[x, y];
            set
            {
                if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(data[x, y], value)) return;
                data[x, y] = value;
                if (isDisposed) return;
                onChangeSubject.OnNext((x, y, value));
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    yield return data[x, y];
        }
        public IEnumerable<(int x, int y, T value)> EnumerateWithPos()
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    yield return (x, y, data[x, y]);
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            onChangeSubject.Dispose();
        }
    }
}