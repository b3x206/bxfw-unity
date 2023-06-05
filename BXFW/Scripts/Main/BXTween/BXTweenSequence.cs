using System;
using System.Collections.Generic;

namespace BXFW.Tweening
{
    /// <summary>
    /// A list of tweens that can be run sequentially or programatically.
    /// </summary>
    /// <typeparam name="TProperty">Type of the list of properties passed.</typeparam>
    [Serializable]
    public class BXTweenSequence<TProperty> : List<TProperty>
        where TProperty : BXTweenPropertyBase
    {
        private ITweenCTX currentCtx;

        public void Run()
        {
            throw new NotImplementedException(@"To make this class feasible, we need some things to be changed:
1 : The ITweenCTX interface should have more methods and should contain most of the methods
2 : idk, maybe i should be no longer lazy
3 : Need a custom array class for more settngs. just cannot inherit 'List' and be done with.");

            //if (Count <= 0)
            //{
            //    throw new NullReferenceException("[BXTweenSequence::Run] The sequence that was run ");
            //}

            //// Run the tweens using daisy chaining?
            //for (int i = 0; i < Count; i++)
            //{
            //    if (i == 0)
            //    {
            //        continue;
            //    }

            //    int prev = i - 1;
            //    //this[prev].IContext.
            //}
        }
    }
}
