import type DecodedToken from './DecodedToken';

export default class TokenInfo {
    id: string;
    expires: Date;
    scopes: string[];

    constructor(decoded: DecodedToken) {
        this.id = decoded.sub;
        this.expires = decoded.exp;
        this.scopes = decoded.scope.split(' ');
    }

    hasPermission(scope: string) {
        return this.scopes.find((s) => s == scope) !== undefined;
    }
}
