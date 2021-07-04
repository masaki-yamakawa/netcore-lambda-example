# netcore-lambda-example

## 事前インストール

Node.js 16.x.x

## Serverless Frameworkインストール

Serverless Frameworkをグローバルインストール
```
npm install -g serverless@2.50.0
```

インストールで証明書エラーとなる場合は以下を設定して再実行
```
# Windows
set NODE_TLS_REJECT_UNAUTHORIZED=0
  or
# Kinux
export NODE_TLS_REJECT_UNAUTHORIZED=0

npm config set strict-ssl false
npm install -g serverless@2.50.0
npm config set strict-ssl true
```

## AWS Lambdaモジュールビルド

全Functionをビルド
```
cd ${rootDir}
build.cmd
```

特定Functionをビルド
```
cd ${rootDir}/${Function}
build.cmd
```

## AWS環境デプロイ事前準備

serverless.ymlが存在するディレクトリへ移動してServerless Frameworkプラグインをインストール（プラグインが変更となった場合にのみ実行）
```
cd ${rootDir}
sls plugin install --name serverless-deployment-bucket
sls plugin install --name serverless-dotenv-plugin
sls plugin install --name serverless-dynamodb-local
```

## AWS環境へのデプロイ

serverless.ymlが存在するディレクトリへ移動してでデプロイ
```
cd ${rootDir}
sls deploy --stage ${stage} --profile ${profile}
```

## AWS環境へデプロイしたモジュールの削除

serverless.ymlが存在するディレクトリへ移動してでデプロイ
```
cd ${rootDir}
sls remove --stage ${stage} --profile ${profile}
```

