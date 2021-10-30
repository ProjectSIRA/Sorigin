import type User from '$lib/types/user';
import type TokenInfo from './TokenInfo';
import type Tokens from './Tokens';

export default interface UserSession {
    tokens: Tokens;
    tokenInfo: TokenInfo;
    expires: Date | undefined;
    user: User;
}
