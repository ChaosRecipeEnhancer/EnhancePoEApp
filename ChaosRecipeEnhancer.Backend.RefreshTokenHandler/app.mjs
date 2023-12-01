﻿/* global fetch, btoa, atob */

import {
    SecretsManagerClient,
    GetSecretValueCommand,
} from "@aws-sdk/client-secrets-manager";

/**
 * This Lambda function retrieves an authentication token from the Path of Exile API.
 * It first retrieves the client secret from AWS Secrets Manager, then uses it along with the
 * provided code and code_verifier to obtain the token.
 */

/* This uses the AWS Secrets Manager to get the secret key for the GGGAUTH API. */
/* Docs: https://docs.aws.amazon.com/secretsmanager/latest/userguide/retrieving-secrets_lambda.html */
const getClientSecret = async () => {
    const client = new SecretsManagerClient({
        region: "us-east-1"
    });

    let response;

    try {
        const secretName = "CRE/Secrets/GGGAuth";
        response = await client.send(
            new GetSecretValueCommand({
                SecretId: secretName,
                VersionStage: "AWSCURRENT" // VersionStage defaults to AWSCURRENT if unspecified
            })
        );
    } catch (error) {
        // For a list of exceptions thrown, see
        // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
        throw error;
    }

    const secretsJson = response.SecretString;
    const parsedJson = JSON.parse(secretsJson); // Parse the entire JSON string
    const clientSecret = parsedJson.clientSecret; // Extract the clientSecret

    return clientSecret;
};


/**
 * Refreshes an existing authentication token from the Path of Exile API.
 * @param {Object} params - Parameters required for the token request.
 * @param {string} params.secretKey - The client secret key.
 * @param {string} params.refreshToken - The refresh token.
 * @returns {Promise<Object>} The token response JSON.
 */
const refreshAuthToken = async ({ secretKey, refreshToken }) => {
    const endpoint = "https://www.pathofexile.com/oauth/token";
    const headers = { "Content-Type": "application/x-www-form-urlencoded" };

    const urlencodedParams = new URLSearchParams();
    urlencodedParams.append("client_id", "chaosrecipeenhancer");
    urlencodedParams.append("client_secret", secretKey);
    urlencodedParams.append("grant_type", "refresh_token");
    urlencodedParams.append("refresh_token", refreshToken);
    urlencodedParams.append("redirect_uri", "https://sandbox.chaos-recipe.com/auth/success");

    //    Debugging Issues
    //    console.log(`getAuthToken --- urlParams:${urlencodedParams.toString()}`);

    const tokenResponse = await fetch(endpoint, { method: "POST", body: urlencodedParams, headers });

    if (!tokenResponse.ok) {
        console.log(`getAuthToken --- Failed to retrieve token: ${tokenResponse.statusText}`);
        throw new Error(`Failed to retrieve token: ${tokenResponse.statusText}`);
    }

    //    Debugging Issues
    //    console.log(`getAuthToken --- tokenResponse:${JSON.stringify(tokenResponse)}`);

    return tokenResponse.json();
};

export const handler = async (event, context) => {
    try {
        const secretKey = await getClientSecret();
        const params = new URLSearchParams(atob(event.body));
        const refreshToken = params.get("refresh_token");

        if (!refreshToken) {
            console.log(`handler --- Error! Missing 'refresh_token' in request.`);
            return {
                statusCode: 400,
                body: JSON.stringify({ message: "Missing 'refresh_token' in request." })
            };
        }

        const token = await refreshAuthToken({ secretKey, refreshToken });

        return {
            statusCode: 200,
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(token)
        };
    } catch (error) {
        console.log(`handler --- Error! ${error.message}`);
        return {
            statusCode: 500,
            body: JSON.stringify({ message: error.message })
        };
    }
};