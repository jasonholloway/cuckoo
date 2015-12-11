var gulp = require('gulp');
var through = require('through2');
var spawn = require('child_process').spawn;
var gutil = require('gulp-util');
var fs = require('fs');


var assemblyInfoPath = '../SharedAssemblyInfo.cs';

var nugetRepos = [
  { source: 'D:/dev/nugetrepo/' }
//  { source: 'https://www.myget.org/F/jasonholloway/api/v2', key: '0fa24222-a0d2-4fe5-b9b7-54c836fa7b0b' }
]


gulp.task('package', [], function() {

  var data = fs.readFileSync(assemblyInfoPath, 'utf8');

  var props = {
    version: data.match(/\[assembly: AssemblyVersion\(\"(.*?)\"\)\]/)[1],
    target: 'debug'
  };

  return gulp.src('**/*.nuspec')
    .pipe(through.obj(function(file, _, done) {
      var s = spawn('nuget', [ 'pack', file.path, '-OutputDirectory', './build/', '-properties', 'version='+props.version+';target='+props.target ]);

      s.stderr.on('data', function(err) {
        done(err);
      });

      s.on('close', function (code) {
        gutil.log(file.path + ' packaged!');
        done();
      });
    }));
})


gulp.task('push', ['package'], function() {
  return gulp.src('./build/*.nupkg')
    .pipe(through.obj(function(file, _, done) {
      for(var repo of nugetRepos) {
        this.push({
          file: file,
          repo: repo
        });
      }
      done();
    }))
    .pipe(through.obj(function(spec, _, done) {      
      var s = spawn('nuget', [ 'push', spec.file.path, spec.repo.key, '-source', spec.repo.source ]);

      s.stderr.on('data', function(err) {
        done(err);
      });

      s.on('close', function(code) {
        gutil.log(spec.file.path + ' pushed to ' + spec.repo.source + '!');
        done();
      });
    }))
})


gulp.task('package-and-push', ['package', 'push']);
