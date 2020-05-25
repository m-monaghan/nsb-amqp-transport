namespace NServiceBus.Transport.Amqp.Sending {
    using System;
    using System.Collections.Generic;
    using global::Amqp;

    public class SenderLinkCache: IDisposable {
        private Dictionary<string, SenderLink> senderLinkCache = new Dictionary<string, SenderLink> ();

        public SenderLinkCache () {
        }

        public SenderLink GetSenderLink( string senderLinkName ) {
            if (this.senderLinkCache.TryGetValue ( $"nsb-{senderLinkName}", out var cachedSenderLink ))
                return cachedSenderLink;

            return null;
        }

        public void CacheSenderLink( string senderLinkName, SenderLink senderLink ) {
            if (this.senderLinkCache.ContainsKey ( $"nsb-{senderLinkName}" )) return;

            this.senderLinkCache.Add ( $"nsb-{senderLinkName}", senderLink );
        }

        public SenderLink CacheSenderLink ( Session session, string senderLinkName, string destination ) {
            if (this.senderLinkCache.ContainsKey ( $"nsb-{senderLinkName}" )) return null;

            var newSenderLink = new SenderLink (
                session,
                $"nsb-{senderLinkName}",
                destination );
            this.CacheSenderLink ( senderLinkName, newSenderLink );
            return newSenderLink;
        }

        public void Dispose () {
            foreach (var senderLink in this.senderLinkCache.Values) {
                senderLink.Close ();
            }
        }
    }
}
