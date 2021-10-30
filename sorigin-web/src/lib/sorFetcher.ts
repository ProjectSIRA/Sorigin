import axios, { AxiosError, AxiosRequestConfig, AxiosResponse, Method } from 'axios';
import type DecodedToken from './models/DecodedToken';
import type Tokens from './models/Tokens';
import TokenInfo from './models/TokenInfo';
import { soriginURL } from './sorigin';
import { sessionStore } from './stores/sessionStore';
import { session } from '$app/stores';
import type UserSession from './models/UserSession';

interface Refresh {
    refreshTokenLifetimeInHours: number;
}

export async function getRefreshTimeInDays() {
    const refreshTokenLifetimeResponse = await get<Refresh>('/refresh');
    if (!refreshTokenLifetimeResponse.data) {
        return 0;
    }

    const timeInHours: number = refreshTokenLifetimeResponse.data.refreshTokenLifetimeInHours;
    const timeInDays: number = timeInHours / 24.0;
    return timeInDays;
}

export async function get<T>(apiURL: string): Promise<SoriginResponse<T>> {
    const sorResponse = new SoriginResponse<T>();
    try {
        const response = await axios.get<T>(soriginURL() + '/api' + apiURL);
        sorResponse.addData(response.data);
    } catch (err) {
        sorResponse.addError(err);
    }
    return sorResponse;
}

export async function getAuthed<T>(apiURL: string, tokens: Tokens) {
    return doAuthedReq<T>(apiURL, tokens, 'GET');
}

export async function postAuthed<T>(apiURL: string, tokens: Tokens, body?: any) {
    return doAuthedReq<T>(apiURL, tokens, 'POST', body);
}

async function doAuthedReq<T>(apiURL: string, tokens: Tokens, method: Method, body?: any) {
    const sorResponse = new SoriginResponse<T>();
    try {
        const response = await axios.request({
            url: soriginURL() + '/api' + apiURL,
            method: method,
            headers: { Authorization: `Bearer ${tokens.token}` },
            data: body
        });

        sorResponse.addData(response.data);
    } catch (err) {
        if (axios.isAxiosError(err)) {
            const error = err as AxiosError;
            if (error.response && error.response.headers['x-token-expired']) {
                const newToken = await getNewToken(tokens.refreshToken!);
                try {
                    const response = await axios.request({
                        url: soriginURL() + '/api' + apiURL,
                        method: method,
                        headers: { Authorization: `Bearer ${newToken}` },
                        data: body
                    });
                    sorResponse.addData(response.data);
                } catch (err) {
                    sorResponse.addError(err);
                }
            }
        }

        if (err.message) sorResponse.addError(err);
    }
    return sorResponse;
}

async function getNewToken(refreshToken: string) {
    const tokenResponse = await get<Tokens>('/auth/refresh?refreshToken=' + refreshToken);
    if (!tokenResponse.data) {
        return null;
    }

    const decoded = jwt_decode(tokenResponse.data.token) as DecodedToken;
    const tokenInfo = new TokenInfo(decoded);
    const newTokens = tokenResponse.data;

    const timeInDays = await getRefreshTimeInDays();

    sessionStore.update((session) => {
        if (session) {
            session.tokenInfo = tokenInfo;
            session.tokens = newTokens;
        }
        return session;
    });

    session.update((session) => {
        if (session && session.data) {
            const data: UserSession = session.data as UserSession;
            data.tokenInfo = tokenInfo;
            data.tokens = newTokens;

            session.refresh(timeInDays);
        }
    });
}

export class SoriginResponse<T> {
    data?: T;
    error?: Error;
    errorMessage?: string;

    addData(data: T): void {
        this.data = data;
    }

    addError(error: Error): void {
        this.error = error;
        this.errorMessage = error.message;
    }
}
function jwt_decode(token: any): any {
    throw new Error('Function not implemented.');
}
