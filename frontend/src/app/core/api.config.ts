/**
 * All HTTP calls go through the dev-server proxy (see proxy.conf.json), so the
 * client uses a relative base and stays same-origin — which keeps the Identity
 * auth cookie working without any CORS gymnastics.
 */
export const API_BASE = '/api';
