export async function get(req) {
    req.locals.user = 'delete'
    return {
        status: 302,
        headers: {
            location: '/'
        }
    }
}