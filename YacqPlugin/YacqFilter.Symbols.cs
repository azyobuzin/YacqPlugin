using System;
using System.Linq.Expressions;
using Dulcet.Twitter;
using Inscribe.Common;
using XSpect.Yacq;
using XSpect.Yacq.Expressions;

namespace YacqPlugin
{
    partial class YacqFilter
    {
        internal static class Symbols
        {
            [YacqSymbol(DispatchTypes.Method, typeof(TwitterStatusBase), "dmsg")]
            public static Expression IsDirectMessage(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Left
                    .Method(s, "is", YacqExpression.TypeCandidate(typeof(TwitterDirectMessage)));
            }

            [YacqSymbol(DispatchTypes.Method, typeof(TwitterStatusBase), "protected")]
            public static Expression IsProtected(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Left
                    .Member(s, "User")
                    .Member(s, "IsProtected");
            }

            [YacqSymbol(DispatchTypes.Method, typeof(TwitterStatusBase), "retweeted")]
            public static Expression IsRetweeted(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Left
                    .Method(s, "as", YacqExpression.TypeCandidate(typeof(TwitterStatus)))
                    .Method(s, "let",
                        YacqExpression.Identifier("_"),
                        YacqExpression.Function(s, "?",
                            YacqExpression.Identifier("_"),
                            YacqExpression.Identifier("_")
                                .Member(s, "RetweetedOriginal")
                        )
                    );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(TwitterStatusBase), "verified")]
            public static Expression IsVerified(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.TypeCandidate(typeof(TwitterHelper))
                    .Method(s, "GetSuggestedUser", YacqExpression.Identifier("it"))
                    .Member(s, "IsVerified");
            }
        }
    }
}
