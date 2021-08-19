import type User from '$lib/types/user'
import cookie from 'cookie'

// This is a mess!
export async function handle({ request, resolve }) {
    const cookies = cookie.parse(request.headers.cookie || '')
    if (cookies.token !== undefined && cookies.token !== null) {
        request.locals.token = cookies.token as string
        request.locals.session_end = cookies.session_end as string
        request.locals.user = JSON.parse(cookies.user) as User
    }
    const response = await resolve(request)

    if (request.locals.user === 'delete') {
        response.headers['set-cookie'] = `user=logout; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT`
        response.headers['set-cookie'] = `session_end=logout; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT`
        response.headers['set-cookie'] = `token=logout; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT`
    }
    else if (request.locals.user !== undefined && cookies.user === undefined) {
        let fourHours = new Date()
        fourHours.setTime(fourHours.getTime() + 4 * 60 * 60 * 1000)
        request.locals.token = request.locals.user.token
        response.headers['set-cookie'] = [
            `user=${JSON.stringify(request.locals.user.user) || ''}; expires=${fourHours.toUTCString()}; Path=/; HttpOnly`,
            `token=${request.locals.user.token || ''}; expires=${fourHours.toUTCString()}; Path=/; HttpOnly`,
            `session_end=pragma; expires=${fourHours.toUTCString()}; Path=/; HttpOnly`
        ]
    }
    else if (request.locals.uu !== undefined && cookies.user !== undefined && cookies.token !== undefined) {
        let expiresIn = cookies.session_end
        response.headers['set-cookie'] = [
            `session_end=pragma; expires=${expiresIn}; Path=/; HttpOnly`,
            `token=${cookies.token || ''}; expires=${expiresIn}; Path=/; HttpOnly`,
            `user=${JSON.stringify(request.locals.uu) || ''}; expires=${expiresIn}; Path=/; HttpOnly`
        ]
    }

    return response
}

export async function getSession(request) {
    return {
        user: request.locals.user,
        token: request.locals.token
    }
}