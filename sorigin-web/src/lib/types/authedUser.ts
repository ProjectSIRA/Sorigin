import type User from './user'

export default interface AuthedUser {
    token: string,
    user: User
}