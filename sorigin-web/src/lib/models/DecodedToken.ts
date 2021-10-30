export default interface DecodedToken {
    sub: string;
    scope: string;
    exp: Date;
    iss: string;
    aud: string;
}
